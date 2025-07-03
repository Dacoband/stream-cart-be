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
using OrderService.Application.Interfaces.IServices;
using Shared.Messaging.Event.OrderEvents;
using MassTransit;

namespace OrderService.Application.Handlers.OrderCommandHandlers
{
    public class UpdateShippingInfoCommandHandler : IRequestHandler<UpdateShippingInfoCommand, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<UpdateShippingInfoCommandHandler> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateShippingInfoCommandHandler(
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient,
            ILogger<UpdateShippingInfoCommandHandler> logger,
            IPublishEndpoint publishEndpoint)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publishEndpoint = publishEndpoint;
        }

        public async Task<OrderDto> Handle(UpdateShippingInfoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating shipping information for order {OrderId}", request.OrderId);

                var order = await _orderRepository.GetByIdAsync(request.OrderId.ToString());
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", request.OrderId);
                    throw new ApplicationException($"Order with ID {request.OrderId} not found");
                }

                order.SetShippingFee(request.ShippingFee, request.ModifiedBy);
                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                var shippingAddressDto = request.ShippingAddress;

                var orderItemDtos = new List<OrderItemDto>();
                if (order.Items != null)
                {
                    foreach (var item in order.Items)
                    {
                        var productDetails = await _productServiceClient.GetProductByIdAsync(item.ProductId);

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
                            Notes = item.Notes,
                            ProductName = productDetails?.ProductName ?? "Unknown Product",
                            ProductImageUrl = productDetails?.ImageUrl ?? string.Empty
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

                _logger.LogInformation("Shipping information updated successfully for order {OrderId}", request.OrderId);
                //pubish OrderChangeEvent to NotificationSevice
                var orderChangEvent = new OrderCreatedOrUpdatedEvent()
                {
                    OrderCode = order.OrderCode,
                    Message = "đã được thay đổi địa chỉ gioa hàng thành công",
                    UserId = request.ModifiedBy,
                };
                await _publishEndpoint.Publish(orderChangEvent);
                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipping information: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}