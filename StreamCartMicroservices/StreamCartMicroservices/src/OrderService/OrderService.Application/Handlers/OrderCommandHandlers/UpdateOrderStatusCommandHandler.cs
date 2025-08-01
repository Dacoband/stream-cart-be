using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderCommands;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Enums;
using System.Collections.Generic;
using Shared.Messaging.Event.OrderEvents;
using MassTransit;
using OrderService.Application.Interfaces;

namespace OrderService.Application.Handlers.OrderCommandHandlers
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IAccountServiceClient _accountServiceClient;

        public UpdateOrderStatusCommandHandler(
            IOrderRepository orderRepository,
            ILogger<UpdateOrderStatusCommandHandler> logger,
            IPublishEndpoint publishEndpoint, IAccountServiceClient accountServiceClient)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publishEndpoint = publishEndpoint;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating order status for order {OrderId} to {NewStatus}", request.OrderId, request.NewStatus);

                // Get the order
                var order = await _orderRepository.GetByIdAsync(request.OrderId.ToString());
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", request.OrderId);
                    throw new ApplicationException($"Order with ID {request.OrderId} not found");
                }
                var message = "";
                switch (request.NewStatus)
                {
                    case OrderStatus.Processing:
                        order.Process(request.ModifiedBy);
                        message = "đang được xử lý bởi hệ thống";
                        break;
                    case OrderStatus.Shipped:
                        order.Ship(order.TrackingCode, request.ModifiedBy);
                        message = "đang được giao đến bạn";
                        break;
                    case OrderStatus.Delivered:
                        order.Deliver(request.ModifiedBy);
                        message = "đã được giao thành công";

                        break;
                    case OrderStatus.Cancelled:
                        order.Cancel(request.ModifiedBy);
                        message = "đã bị hủy";

                        break;
                    default:
                        _logger.LogWarning("Unsupported order status transition to {NewStatus}", request.NewStatus);
                        throw new InvalidOperationException($"Unsupported status transition to {request.NewStatus}");
                }

                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                var shippingAddressDto = new ShippingAddressDto
                {
                    FullName = order.ToName,
                    Phone = order.ToPhone,
                    AddressLine1 = order.ToAddress,
                    Ward = order.ToWard,
                    City = order.ToProvince,
                    State = order.ToDistrict,
                    PostalCode = order.ToPostalCode,
                    Country = "Vietnam", 
                    IsDefault = false
                };

                // Convert items to DTOs
                var orderItemDtos = new List<OrderItemDto>();
                if (order.Items != null)
                {
                    foreach (var item in order.Items)
                    {
                        orderItemDtos.Add(new OrderItemDto
                        {
                            Id = item.Id,
                            OrderId = item.OrderId,
                            ProductId = item.ProductId,
                            VariantId = item.VariantId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            DiscountAmount = item.DiscountAmount,
                            TotalPrice = item.TotalPrice,
                            Notes = item.Notes
                        });
                    }
                }

                var orderDto = new OrderDto
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    AccountId = order.AccountId,
                    ShopId = order.ShopId,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.OrderStatus,
                    PaymentStatus = order.PaymentStatus,
                    ShippingAddress = shippingAddressDto,
                    ShippingProviderId = order.ShippingProviderId,
                    ShippingFee = order.ShippingFee,
                    TotalPrice = order.TotalPrice,
                    DiscountAmount = order.DiscountAmount,
                    FinalAmount = order.FinalAmount,
                    CustomerNotes = order.CustomerNotes,
                    TrackingCode = order.TrackingCode,
                    EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                    ActualDeliveryDate = order.ActualDeliveryDate,
                    LivestreamId = order.LivestreamId,
                    Items = orderItemDtos
                };

                _logger.LogInformation("Order status updated successfully for order {OrderId}", request.OrderId);
                //pubish OrderChangeEvent to NotificationSevice
                //var orderChangEvent = new OrderCreatedOrUpdatedEvent()
                //{
                //    OrderCode = order.OrderCode,
                //    Message = message,
                //    UserId = request.ModifiedBy,
                //};
                //await _publishEndpoint.Publish(orderChangEvent);
                //var shopAccount = await _accountServiceClient.GetAccountByShopIdAsync(order.ShopId);
                //foreach (var acc in shopAccount)
                //{
                //    var ordChangeEvent = new OrderCreatedOrUpdatedEvent()
                //    {
                //        OrderCode = order.OrderCode,
                //        Message = message,
                //        UserId = acc.Id.ToString(),
                //    };
                //    await _publishEndpoint.Publish(ordChangeEvent);
                //}
                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}