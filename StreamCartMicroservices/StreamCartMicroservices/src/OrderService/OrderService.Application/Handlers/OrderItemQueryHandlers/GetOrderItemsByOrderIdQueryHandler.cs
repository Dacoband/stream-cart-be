using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Application.Queries.OrderItemQueries;

namespace OrderService.Application.Handlers.OrderItemQueryHandlers
{
    public class GetOrderItemsByOrderIdQueryHandler : IRequestHandler<GetOrderItemsByOrderIdQuery, IEnumerable<OrderItemDto>>
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetOrderItemsByOrderIdQueryHandler> _logger;

        public GetOrderItemsByOrderIdQueryHandler(
            IOrderItemRepository orderItemRepository,
            IProductServiceClient productServiceClient,
            ILogger<GetOrderItemsByOrderIdQueryHandler> logger)
        {
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<OrderItemDto>> Handle(GetOrderItemsByOrderIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting order items for order {OrderId}", request.OrderId);

                var orderItems = await _orderItemRepository.GetByOrderIdAsync(request.OrderId);
                if (orderItems == null || !orderItems.Any())
                {
                    _logger.LogInformation("No order items found for order {OrderId}", request.OrderId);
                    return new List<OrderItemDto>();
                }

                var orderItemDtos = new List<OrderItemDto>();

                foreach (var item in orderItems)
                {
                    // Get product details with enhanced error handling
                    string productName = "Unknown Product";
                    string productImageUrl = string.Empty;

                    try
                    {
                        if (item.ProductId != Guid.Empty)
                        {
                            var productDetails = await _productServiceClient.GetProductByIdAsync(item.ProductId);
                            if (productDetails != null)
                            {
                                productName = productDetails.ProductName ?? "Unknown Product";
                                productImageUrl = productDetails.PrimaryImageUrl ?? string.Empty;
                            }
                        }
                    }
                    catch (Exception productEx)
                    {
                        _logger.LogWarning(productEx, "Failed to get product details for ProductId {ProductId} in order {OrderId}",
                            item.ProductId, request.OrderId);
                        // Continue with default values
                    }

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
                        Notes = item.Notes ?? string.Empty,
                        RefundRequestId = item.RefundRequestId,
                        ProductName = productName,
                        ProductImageUrl = productImageUrl
                    });
                }

                _logger.LogInformation("Successfully retrieved {Count} order items for order {OrderId}",
                    orderItemDtos.Count, request.OrderId);

                return orderItemDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order items by order ID {OrderId}: {ErrorMessage}",
                    request.OrderId, ex.Message);
                throw;
            }
        }
    }
}