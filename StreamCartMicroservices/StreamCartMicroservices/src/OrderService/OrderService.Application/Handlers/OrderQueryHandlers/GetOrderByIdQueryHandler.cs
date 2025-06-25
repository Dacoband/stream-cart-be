using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Application.Queries.OrderQueries;
using System.Collections.Generic;

namespace OrderService.Application.Handlers.OrderQueryHandlers
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetOrderByIdQueryHandler> _logger;

        public GetOrderByIdQueryHandler(
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient,
            ILogger<GetOrderByIdQueryHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting order by ID {OrderId}", request.OrderId);

                var order = await _orderRepository.GetByIdAsync(request.OrderId.ToString());
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", request.OrderId);
                    return null;
                }

                var shippingAddressDto = new ShippingAddressDto
                {
                    FullName = order.ToName,
                    Phone = order.ToPhone,
                    AddressLine1 = order.ToAddress,
                    AddressLine2 = order.ToWard,
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

                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by ID: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}