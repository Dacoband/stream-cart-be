using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Data;
using Shared.Common.Data.Repositories;
using Shared.Common.Domain.Bases;

namespace OrderService.Infrastructure.Repositories
{
    public class OrderItemRepository : EfCoreGenericRepository<OrderItem>, IOrderItemRepository
    {
        private readonly OrderContext _orderContext;
        private readonly ILogger<OrderItemRepository> _logger;

        public OrderItemRepository(OrderContext dbContext, ILogger<OrderItemRepository> logger) : base(dbContext)
        {
            _orderContext = dbContext;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId)
        {
            try
            {
                return await _orderContext.OrderItems
                    .Where(i => i.OrderId == orderId && !i.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order items for order {OrderId}", orderId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrderItem>> GetByProductIdAsync(Guid productId)
        {
            try
            {
                return await _orderContext.OrderItems
                    .Where(i => i.ProductId == productId && !i.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order items for product {ProductId}", productId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrderItem>> GetByVariantIdAsync(Guid variantId)
        {
            try
            {
                return await _orderContext.OrderItems
                    .Where(i => i.VariantId == variantId && !i.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order items for variant {VariantId}", variantId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrderItem>> GetByRefundRequestIdAsync(Guid refundRequestId)
        {
            try
            {
                return await _orderContext.OrderItems
                    .Where(i => i.RefundRequestId == refundRequestId && !i.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order items for refund request {RefundRequestId}", refundRequestId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductSalesStatistics>> GetShopSalesStatisticsAsync(
            Guid shopId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? topProductsLimit = null)
        {
            try
            {
                var query = from orders in _orderContext.Orders
                            join items in _orderContext.OrderItems
                            on orders.Id equals items.OrderId
                            where orders.ShopId == shopId && !items.IsDeleted && !orders.IsDeleted
                            select new { Order = orders, Item = items };

                if (startDate.HasValue)
                {
                    query = query.Where(x => x.Order.OrderDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    var adjustedEndDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(x => x.Order.OrderDate <= adjustedEndDate);
                }

                var results = await query.ToListAsync();

                var productGroups = results
                    .GroupBy(x => x.Item.ProductId);

                var statistics = new List<ProductSalesStatistics>();
                foreach (var group in productGroups)
                {
                    var productId = group.Key;
                    var items = group.Select(x => x.Item).ToList();

                    var totalQuantity = items.Sum(i => i.Quantity);
                    var totalRevenue = items.Sum(i => i.TotalPrice);

                    var averageUnitPrice = totalQuantity > 0
                        ? items.Sum(i => i.UnitPrice * i.Quantity) / totalQuantity
                        : 0;

                    var averageDiscount = items.Any() && items.Sum(i => i.DiscountAmount) > 0
                        ? items.Sum(i => i.DiscountAmount) / items.Count
                        : 0;

                    var orderCount = items.Select(i => i.OrderId).Distinct().Count();

                    var refundCount = items.Count(i => i.RefundRequestId.HasValue);

                    var variantQuantities = items
                        .Where(i => i.VariantId.HasValue)
                        .GroupBy(i => i.VariantId.Value)
                        .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

                    statistics.Add(new ProductSalesStatistics
                    {
                        ProductId = productId,
                        TotalQuantitySold = totalQuantity,
                        TotalRevenue = totalRevenue,
                        AverageUnitPrice = averageUnitPrice,
                        AverageDiscount = averageDiscount,
                        VariantQuantities = variantQuantities,
                        OrderCount = orderCount,
                        RefundCount = refundCount
                    });
                }

                if (topProductsLimit.HasValue && topProductsLimit.Value > 0)
                {
                    statistics = statistics
                        .OrderByDescending(s => s.TotalRevenue)
                        .Take(topProductsLimit.Value)
                        .ToList();
                }

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales statistics for shop {ShopId}", shopId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ProductSalesStatistics> GetProductSalesStatisticsAsync(
            Guid productId, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                var orderItems = _orderContext.OrderItems
                    .Where(i => i.ProductId == productId && !i.IsDeleted)
                    .AsQueryable();

                if (startDate.HasValue || endDate.HasValue)
                {
                    var orders = _orderContext.Orders.AsQueryable();
                    
                    if (startDate.HasValue)
                    {
                        orders = orders.Where(o => o.OrderDate >= startDate.Value);
                    }
                    
                    if (endDate.HasValue)
                    {
                        var adjustedEndDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                        orders = orders.Where(o => o.OrderDate <= adjustedEndDate);
                    }
                    
                    var orderIds = orders.Select(o => o.Id);
                    orderItems = orderItems.Where(i => orderIds.Contains(i.OrderId));
                }

                var items = await orderItems.ToListAsync();

                if (!items.Any())
                {
                    return new ProductSalesStatistics
                    {
                        ProductId = productId,
                        TotalQuantitySold = 0,
                        TotalRevenue = 0,
                        AverageUnitPrice = 0,
                        AverageDiscount = 0,
                        OrderCount = 0,
                        RefundCount = 0
                    };
                }

                var totalQuantity = items.Sum(i => i.Quantity);
                var totalRevenue = items.Sum(i => i.TotalPrice);
                
                var averageUnitPrice = items.Any() 
                    ? items.Sum(i => i.UnitPrice * i.Quantity) / totalQuantity
                    : 0;
                
                var averageDiscount = items.Any() && items.Sum(i => i.DiscountAmount) > 0 
                    ? items.Sum(i => i.DiscountAmount) / items.Count
                    : 0;

                var orderCount = items.Select(i => i.OrderId).Distinct().Count();
                
                var refundCount = items.Count(i => i.RefundRequestId.HasValue);

                var variantQuantities = items
                    .Where(i => i.VariantId.HasValue)
                    .GroupBy(i => i.VariantId.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

                return new ProductSalesStatistics
                {
                    ProductId = productId,
                    TotalQuantitySold = totalQuantity,
                    TotalRevenue = totalRevenue,
                    AverageUnitPrice = averageUnitPrice,
                    AverageDiscount = averageDiscount,
                    VariantQuantities = variantQuantities,
                    OrderCount = orderCount,
                    RefundCount = refundCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales statistics for product {ProductId}", productId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrderItem>> GetByOrderIdsAsync(IEnumerable<Guid> orderIds)
        {
            try
            {
                return await _orderContext.OrderItems
                    .Where(i => orderIds.Contains(i.OrderId) && !i.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order items for multiple orders");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PagedResult<OrderItem>> GetPagedOrderItemsAsync(
            Guid? orderId = null,
            Guid? productId = null,
            Guid? variantId = null,
            int? minQuantity = null,
            decimal? minUnitPrice = null,
            decimal? maxUnitPrice = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                // Start with basic query
                IQueryable<OrderItem> query = _orderContext.OrderItems
                    .Where(i => !i.IsDeleted);

                // Apply filters
                if (orderId.HasValue)
                {
                    query = query.Where(i => i.OrderId == orderId.Value);
                }

                if (productId.HasValue)
                {
                    query = query.Where(i => i.ProductId == productId.Value);
                }

                if (variantId.HasValue)
                {
                    query = query.Where(i => i.VariantId == variantId.Value);
                }

                if (minQuantity.HasValue)
                {
                    query = query.Where(i => i.Quantity >= minQuantity.Value);
                }

                if (minUnitPrice.HasValue)
                {
                    query = query.Where(i => i.UnitPrice >= minUnitPrice.Value);
                }

                if (maxUnitPrice.HasValue)
                {
                    query = query.Where(i => i.UnitPrice <= maxUnitPrice.Value);
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination with ordering
                var orderItems = await query
                    .OrderBy(i => i.OrderId)
                    .ThenBy(i => i.ProductId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Create paged result
                return new PagedResult<OrderItem>
                {
                    Items = orderItems,
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    CurrentPage = pageNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged order items");
                throw;
            }
        }
    }
}
