using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Queries.OrderItemQueries;

namespace OrderService.Application.Handlers.OrderItemQueryHandlers
{
    public class GetShopSalesStatisticsQueryHandler : IRequestHandler<GetShopSalesStatisticsQuery, IEnumerable<ProductSalesStatisticsDto>>
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ILogger<GetShopSalesStatisticsQueryHandler> _logger;

        public GetShopSalesStatisticsQueryHandler(
            IOrderItemRepository orderItemRepository,
            ILogger<GetShopSalesStatisticsQueryHandler> logger)
        {
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ProductSalesStatisticsDto>> Handle(GetShopSalesStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting shop sales statistics for shop {ShopId}, between {StartDate} and {EndDate}, top {TopProductsLimit} products",
                    request.ShopId, request.StartDate, request.EndDate, request.TopProductsLimit);

                var statistics = await _orderItemRepository.GetShopSalesStatisticsAsync(
                    request.ShopId,
                    request.StartDate,
                    request.EndDate,
                    request.TopProductsLimit);

                if (statistics == null)
                {
                    _logger.LogWarning("No sales statistics found for shop {ShopId}", request.ShopId);
                    return new List<ProductSalesStatisticsDto>();
                }

                // Convert from ProductSalesStatistics to ProductSalesStatisticsDto
                var statisticsDtos = statistics.Select(stat => new ProductSalesStatisticsDto
                {
                    ProductId = stat.ProductId,
                    TotalQuantitySold = stat.TotalQuantitySold,
                    TotalRevenue = stat.TotalRevenue,
                    AverageUnitPrice = stat.AverageUnitPrice,
                    AverageDiscount = stat.AverageDiscount,
                    VariantQuantities = stat.VariantQuantities,
                    OrderCount = stat.OrderCount,
                    RefundCount = stat.RefundCount,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                }).ToList();

                return statisticsDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shop sales statistics: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}