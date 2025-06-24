using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Data;
using Shared.Common.Data.Repositories;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Repositories
{
    public class OrderRepository : EfCoreGenericRepository<Orders>, IOrderRepository
    {
        private readonly OrderContext _orderContext;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(OrderContext dbContext, ILogger<OrderRepository> logger) : base(dbContext)
        {
            _orderContext = dbContext;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Orders>> GetByAccountIdAsync(Guid accountId)
        {
            try
            {
                return await _orderContext.Orders
                    .Where(o => o.AccountId == accountId && !o.IsDeleted)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for account {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Orders>> GetByShopIdAsync(Guid shopId)
        {
            try
            {
                return await _orderContext.Orders
                    .Where(o => o.ShopId == shopId && !o.IsDeleted)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for shop {ShopId}", shopId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Orders>> GetByOrderStatusAsync(OrderStatus status)
        {
            try
            {
                return await _orderContext.Orders
                    .Where(o => o.OrderStatus == status && !o.IsDeleted)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders with status {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Orders>> GetByPaymentStatusAsync(PaymentStatus status)
        {
            try
            {
                return await _orderContext.Orders
                    .Where(o => o.PaymentStatus == status && !o.IsDeleted)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders with payment status {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Orders?> GetByOrderCodeAsync(string orderCode)
        {
            try
            {
                return await _orderContext.Orders
                    .FirstOrDefaultAsync(o => o.OrderCode == orderCode && !o.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with code {OrderCode}", orderCode);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Orders>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var adjustedEndDate = endDate.Date.AddDays(1).AddTicks(-1);
                
                return await _orderContext.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= adjustedEndDate && !o.IsDeleted)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders between {StartDate} and {EndDate}", startDate, endDate);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Orders>> GetByShippingProviderAsync(Guid shippingProviderId)
        {
            try
            {
                return await _orderContext.Orders
                    .Where(o => o.ShippingProviderId == shippingProviderId && !o.IsDeleted)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for shipping provider {ProviderId}", shippingProviderId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Orders>> GetByLivestreamAsync(Guid livestreamId)
        {
            try
            {
                return await _orderContext.Orders
                    .Where(o => o.LivestreamId == livestreamId && !o.IsDeleted)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PagedResult<Orders>> GetPagedOrdersAsync(
            Guid? accountId = null,
            Guid? shopId = null,
            OrderStatus? orderStatus = null,
            PaymentStatus? paymentStatus = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                // Start with basic query
                IQueryable<Orders> query = _orderContext.Orders
                    .Where(o => !o.IsDeleted);

                // Apply filters
                if (accountId.HasValue)
                {
                    query = query.Where(o => o.AccountId == accountId.Value);
                }

                if (shopId.HasValue)
                {
                    query = query.Where(o => o.ShopId == shopId.Value);
                }

                if (orderStatus.HasValue)
                {
                    query = query.Where(o => o.OrderStatus == orderStatus.Value);
                }

                if (paymentStatus.HasValue)
                {
                    query = query.Where(o => o.PaymentStatus == paymentStatus.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    var adjustedEndDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(o => o.OrderDate <= adjustedEndDate);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(o => 
                        o.OrderCode.ToLower().Contains(searchTerm) || 
                        o.CustomerNotes.ToLower().Contains(searchTerm) ||
                        o.ToName.ToLower().Contains(searchTerm));
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination with ordering
                var orders = await query
                    .OrderByDescending(o => o.OrderDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Create paged result
                return new PagedResult<Orders>
                {
                    Items = orders,
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    CurrentPage = pageNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged orders");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderStatistics> GetOrderStatisticsAsync(
            Guid shopId, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                // Start with basic query
                IQueryable<Orders> query = _orderContext.Orders
                    .Where(o => o.ShopId == shopId && !o.IsDeleted);

                // Apply date filters if provided
                if (startDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    var adjustedEndDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(o => o.OrderDate <= adjustedEndDate);
                }

                // Get all relevant orders in one query
                var orders = await query.ToListAsync();

                // Calculate statistics
                var totalOrders = orders.Count;
                var ordersByStatus = orders
                    .GroupBy(o => o.OrderStatus)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                var totalRevenue = orders.Sum(o => o.FinalAmount);
                var totalCommissionFees = orders.Sum(o => o.CommissionFee);
                var netRevenue = totalRevenue - totalCommissionFees;
                
                // Calculate average order value
                var averageOrderValue = totalOrders > 0 
                    ? totalRevenue / totalOrders 
                    : 0;

                // Get total items sold (requires additional query to load order items)
                var orderIds = orders.Select(o => o.Id).ToList();
                var totalItemsSold = await _orderContext.OrderItems
                    .Where(i => orderIds.Contains(i.OrderId) && !i.IsDeleted)
                    .SumAsync(i => i.Quantity);

                // Create statistics result
                return new OrderStatistics
                {
                    TotalOrders = totalOrders,
                    OrdersByStatus = ordersByStatus,
                    TotalRevenue = totalRevenue,
                    TotalCommissionFees = totalCommissionFees,
                    NetRevenue = netRevenue,
                    AverageOrderValue = averageOrderValue,
                    TotalItemsSold = totalItemsSold
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order statistics for shop {ShopId}", shopId);
                throw;
            }
        }
    }
}
