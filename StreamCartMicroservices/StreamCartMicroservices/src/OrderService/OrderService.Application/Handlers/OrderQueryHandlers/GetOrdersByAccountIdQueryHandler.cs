using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Application.Queries.OrderQueries;
using Shared.Common.Domain.Bases;

namespace OrderService.Application.Handlers.OrderQueryHandlers
{
    public class GetOrdersByAccountIdQueryHandler : IRequestHandler<GetOrdersByAccountIdQuery, PagedResult<OrderDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetOrdersByAccountIdQueryHandler> _logger;

        public GetOrdersByAccountIdQueryHandler(
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient,
            ILogger<GetOrdersByAccountIdQueryHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<OrderDto>> Handle(GetOrdersByAccountIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting orders for account {AccountId}", request.AccountId);

                // Sử dụng phương thức GetPagedOrdersAsync thay vì GetByAccountIdAsync để hỗ trợ phân trang
                var pagedOrders = await _orderRepository.GetPagedOrdersAsync(
                    accountId: request.AccountId,
                    shopId: null,
                    orderStatus: null,
                    paymentStatus: null,
                    startDate: null,
                    endDate: null,
                    searchTerm: null,
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize);

                if (pagedOrders == null || !pagedOrders.Items.Any())
                {
                    _logger.LogInformation("No orders found for account {AccountId}", request.AccountId);
                    return new PagedResult<OrderDto>(
                        Enumerable.Empty<OrderDto>(),
                        0,
                        request.PageNumber,
                        request.PageSize);
                }

                var orderDtos = new List<OrderDto>();

                foreach (var order in pagedOrders.Items)
                {
                    // Create shipping address DTO
                    var shippingAddressDto = new ShippingAddressDto
                    {
                        FullName = order.ToName,
                        Phone = order.ToPhone,
                        AddressLine1 = order.ToAddress,
                        Ward = order.ToWard,
                        City = order.ToProvince,
                        State = order.ToDistrict,
                        PostalCode = order.ToPostalCode,
                        Country = "Vietnam", // Default value
                        IsDefault = false
                    };

                    // Convert order items to DTOs
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

                    // Map to OrderDto
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

                    orderDtos.Add(orderDto);
                }

                // Tạo và trả về đối tượng PagedResult<OrderDto>
                return new PagedResult<OrderDto>(
                    orderDtos,
                    pagedOrders.TotalCount,
                    pagedOrders.CurrentPage,
                    pagedOrders.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by account ID: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}