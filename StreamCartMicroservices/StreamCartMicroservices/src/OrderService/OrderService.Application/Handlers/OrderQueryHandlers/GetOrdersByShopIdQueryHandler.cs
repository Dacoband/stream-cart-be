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
using Shared.Common.Domain.Bases;

namespace OrderService.Application.Handlers.OrderQueryHandlers
{
    /// <summary>
    /// Handler for getting orders by shop ID with pagination
    /// </summary>
    public class GetOrdersByShopIdQueryHandler : IRequestHandler<GetOrdersByShopIdQuery, PagedResult<OrderDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetOrdersByShopIdQueryHandler> _logger;

        public GetOrdersByShopIdQueryHandler(
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient,
            ILogger<GetOrdersByShopIdQueryHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<OrderDto>> Handle(GetOrdersByShopIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting orders for shop {ShopId} (Page {PageNumber}, Size {PageSize})",
                    request.ShopId, request.PageNumber, request.PageSize);

                var pagedOrders = await _orderRepository.GetPagedOrdersAsync(
                    accountId: null,
                    shopId: request.ShopId,
                    orderStatus: null,
                    paymentStatus: null,
                    startDate: null,
                    endDate: null,
                    searchTerm: null,
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize);

                var orderDtos = new List<OrderDto>();

                foreach (var order in pagedOrders.Items)
                {               
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

                    orderDtos.Add(orderDto);
                }

                return new PagedResult<OrderDto>
                {
                    Items = orderDtos,
                    CurrentPage = pagedOrders.CurrentPage,
                    PageSize = pagedOrders.PageSize,
                    TotalCount = pagedOrders.TotalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for shop {ShopId}: {ErrorMessage}", request.ShopId, ex.Message);
                throw;
            }
        }
    }
}