using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Queries.OrderItemQueries;

namespace OrderService.Application.Handlers.OrderItemQueryHandlers
{
    public class GetProductSalesStatisticsQueryHandler : IRequestHandler<GetProductSalesStatisticsQuery, ProductSalesStatisticsDto>
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderRepository _orderRepository; // Added for filtering by shop if needed
        private readonly ILogger<GetProductSalesStatisticsQueryHandler> _logger;

        public GetProductSalesStatisticsQueryHandler(
            IOrderItemRepository orderItemRepository,
            IOrderRepository orderRepository,
            ILogger<GetProductSalesStatisticsQueryHandler> logger)
        {
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ProductSalesStatisticsDto> Handle(GetProductSalesStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting sales statistics for product {ProductId}, shop {ShopId}, between {StartDate} and {EndDate}",
                    request.ProductId, request.ShopId, request.StartDate, request.EndDate);

                var statistics = await _orderItemRepository.GetProductSalesStatisticsAsync(
                    request.ProductId,
                    request.StartDate,
                    request.EndDate);
                if (request.ShopId.HasValue && statistics != null)
                {
                    _logger.LogInformation("Filtering statistics for shop {ShopId}", request.ShopId);
                    // Note: We would need to implement shop filtering logic here
                    // This is a placeholder for that logic

                    // Example approach:
                    // 1. Get all orders for the shop
                    // 2. Get all order items for these orders that match the product
                    // 3. Recalculate statistics based on these filtered items

                    // For now, we'll just return the unfiltered statistics with a warning
                    _logger.LogWarning("Shop filtering for product statistics is not fully implemented");
                }

                if (statistics == null)
                {
                    _logger.LogWarning("No sales statistics found for product {ProductId}", request.ProductId);

                    return new ProductSalesStatisticsDto
                    {
                        ProductId = request.ProductId,
                        TotalQuantitySold = 0,
                        TotalRevenue = 0,
                        AverageUnitPrice = 0,
                        AverageDiscount = 0,
                        VariantQuantities = new System.Collections.Generic.Dictionary<Guid, int>(),
                        OrderCount = 0,
                        RefundCount = 0,
                        StartDate = request.StartDate,
                        EndDate = request.EndDate
                    };
                }

                var statisticsDto = new ProductSalesStatisticsDto
                {
                    ProductId = statistics.ProductId,
                    TotalQuantitySold = statistics.TotalQuantitySold,
                    TotalRevenue = statistics.TotalRevenue,
                    AverageUnitPrice = statistics.AverageUnitPrice,
                    AverageDiscount = statistics.AverageDiscount,
                    VariantQuantities = statistics.VariantQuantities,
                    OrderCount = statistics.OrderCount,
                    RefundCount = statistics.RefundCount,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                };

                return statisticsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product sales statistics: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}