using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderCommands;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using System.Collections.Generic;
using Shared.Messaging.Event.OrderEvents;
using MassTransit;
using OrderService.Application.Interfaces;

namespace OrderService.Application.Handlers.OrderCommandHandlers
{
    public class UpdateTrackingCodeCommandHandler : IRequestHandler<UpdateTrackingCodeCommand, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<UpdateTrackingCodeCommandHandler> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IAccountServiceClient _accountServiceClient;

        public UpdateTrackingCodeCommandHandler(
            IOrderRepository orderRepository,
            ILogger<UpdateTrackingCodeCommandHandler> logger,
            IPublishEndpoint publishEndpoint, IAccountServiceClient accountServiceClient)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publishEndpoint = publishEndpoint;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<OrderDto> Handle(UpdateTrackingCodeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating tracking code for order {OrderId}", request.OrderId);

                var order = await _orderRepository.GetByIdAsync(request.OrderId.ToString());
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", request.OrderId);
                    throw new ApplicationException($"Order with ID {request.OrderId} not found");
                }

                order.SetTrackingCode(request.TrackingCode, request.ModifiedBy);
                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                var shippingAddressDto = new ShippingAddressDto
                {
                    FullName = order.ToName,
                    Phone = order.ToPhone,
                    AddressLine1 = order.ToAddress,
                   // AddressLine2 = order.ToWard,
                    Ward = order.ToWard,
                    City = order.ToProvince,
                    State = order.ToDistrict,
                    PostalCode = order.ToPostalCode,
                    Country = "Vietnam", 
                    IsDefault = false
                };

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
                //pubish OrderChangeEvent to NotificationSevice
                var orderChangEvent = new OrderCreatedOrUpdatedEvent()
                {
                    OrderCode = order.OrderCode,
                    Message = "đã được cập nhật mã vận đơn thành công",
                    UserId = request.ModifiedBy,
                };
                
                await _publishEndpoint.Publish(orderChangEvent);
                var shopAccount = await _accountServiceClient.GetAccountByShopIdAsync(order.ShopId);
                foreach (var acc in shopAccount)
                {
                    var ordChangeEvent = new OrderCreatedOrUpdatedEvent()
                    {
                        OrderCode = order.OrderCode,
                        Message = "vừa được cập nhật mã vận đơn",
                        UserId = acc.Id.ToString(),
                    };
                    await _publishEndpoint.Publish(ordChangeEvent);
                }
                _logger.LogInformation("Tracking code updated successfully for order {OrderId}", request.OrderId);
                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tracking code: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}