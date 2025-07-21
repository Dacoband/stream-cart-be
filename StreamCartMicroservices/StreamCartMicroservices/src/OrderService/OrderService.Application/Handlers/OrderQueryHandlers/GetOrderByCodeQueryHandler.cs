using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Application.Queries.OrderQueries;

namespace OrderService.Application.Handlers.OrderQueryHandlers
{
    public class GetOrderByCodeQueryHandler : IRequestHandler<GetOrderByCodeQuery, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetOrderByCodeQueryHandler> _logger;

        public GetOrderByCodeQueryHandler(
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient,
            ILogger<GetOrderByCodeQueryHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderDto> Handle(GetOrderByCodeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting order by code {OrderCode}", request.OrderCode);

                var order = await _orderRepository.GetByOrderCodeAsync(request.OrderCode);
                if (order == null)
                {
                    _logger.LogWarning("Order with code {OrderCode} not found", request.OrderCode);
                    return null;
                }

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
                _logger.LogError(ex, "Error getting order by code: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}