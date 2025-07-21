using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderCommands;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Events;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Domain.Enums;
using Shared.Messaging.Event.OrderEvents;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Handlers.OrderCommandHandlers
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient; 
        private readonly ILogger<CancelOrderCommandHandler> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IAccountServiceClient _accountServiceClient;

        public CancelOrderCommandHandler(
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient, 
            ILogger<CancelOrderCommandHandler> logger, IPublishEndpoint publishEndpoint, IAccountServiceClient accountServiceClient)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publishEndpoint = publishEndpoint;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Cancelling order {OrderId}", request.OrderId);

                var order = await _orderRepository.GetByIdAsync(request.OrderId.ToString());
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", request.OrderId);
                    throw new ApplicationException($"Order with ID {request.OrderId} not found");
                }

                if (order.OrderStatus == OrderStatus.Delivered || order.OrderStatus == OrderStatus.Cancelled)
                {
                    _logger.LogWarning("Cannot cancel order {OrderId} with status {Status}", request.OrderId, order.OrderStatus);
                    throw new InvalidOperationException($"Cannot cancel order with status {order.OrderStatus}");
                }

                if (!string.IsNullOrEmpty(request.CancelReason))
                {
                    // Nếu Orders không có phương thức này, bạn có thể cần bổ sung vào Orders class
                    // order.SetCancelReason(request.CancelReason);
                }

 
                order.Cancel(request.CancelledBy);

                // Save changes
                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

              
                var shippingAddressDto = new ShippingAddressDto
                {
                    FullName = order.ToName,
                    Phone = order.ToPhone,
                    AddressLine1 = order.ToAddress,
                    Ward = order.ToWard, // Sử dụng ToWard cho AddressLine2
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
                        // Lấy thông tin sản phẩm từ ProductService
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

                // Convert to DTO
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
                    LivestreamId = order.LivestreamId,
                    EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                    ActualDeliveryDate = order.ActualDeliveryDate,
                    Items = orderItemDtos
                };
                //pubish OrderChangeEvent to NotificationSevice
                var orderChangEvent = new OrderCreatedOrUpdatedEvent()
                {
                    OrderCode = order.OrderCode,
                    Message = "đã bị hủy",
                    UserId = request.CancelledBy,
                };
                await _publishEndpoint.Publish(orderChangEvent);
                var shopAccount = await _accountServiceClient.GetAccountByShopIdAsync(order.ShopId);
                foreach (var acc in shopAccount)
                {
                    var ordChangeEvent = new OrderCreatedOrUpdatedEvent()
                    {
                        OrderCode = order.OrderCode,
                        Message = "đã bị hủy",
                        UserId = acc.Id.ToString(),
                    };
                    await _publishEndpoint.Publish(ordChangeEvent);
                }
                _logger.LogInformation("Order {OrderId} cancelled successfully", request.OrderId);
                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}