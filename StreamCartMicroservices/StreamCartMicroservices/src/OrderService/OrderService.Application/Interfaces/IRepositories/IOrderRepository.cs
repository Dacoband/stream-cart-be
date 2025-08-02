using MassTransit.Transports;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces.IRepositories
{
    public interface IOrderRepository : IGenericRepository<Orders>
    {
        /// <summary>
        /// Gets orders by account ID (customer)
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Collection of orders</returns>
        Task<IEnumerable<Orders>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Gets orders by shop ID
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>Collection of orders</returns>
        Task<IEnumerable<Orders>> GetByShopIdAsync(Guid shopId);

        /// <summary>
        /// Gets orders by order status
        /// </summary>
        /// <param name="status">Order status</param>
        /// <returns>Collection of orders</returns>
        Task<IEnumerable<Orders>> GetByOrderStatusAsync(OrderStatus status);

        /// <summary>
        /// Gets orders by payment status
        /// </summary>
        /// <param name="status">Payment status</param>
        /// <returns>Collection of orders</returns>
        Task<IEnumerable<Orders>> GetByPaymentStatusAsync(PaymentStatus status);

        /// <summary>
        /// Gets an order by its order code
        /// </summary>
        /// <param name="orderCode">Order code</param>
        /// <returns>Order if found, null otherwise</returns>
        Task<Orders?> GetByOrderCodeAsync(string orderCode);

        /// <summary>
        /// Gets orders created within a date range
        /// </summary>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>Collection of orders</returns>
        Task<IEnumerable<Orders>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets orders for a specific shipping provider
        /// </summary>
        /// <param name="shippingProviderId">Shipping provider ID</param>
        /// <returns>Collection of orders</returns>
        Task<IEnumerable<Orders>> GetByShippingProviderAsync(Guid shippingProviderId);

        /// <summary>
        /// Gets orders related to a specific livestream
        /// </summary>
        /// <param name="livestreamId">Livestream ID</param>
        /// <returns>Collection of orders</returns>
        Task<IEnumerable<Orders>> GetByLivestreamAsync(Guid livestreamId);

        /// <summary>
        /// Gets orders with paging and multiple filter options
        /// </summary>
        /// <param name="accountId">Optional account ID filter</param>
        /// <param name="shopId">Optional shop ID filter</param>
        /// <param name="orderStatus">Optional order status filter</param>
        /// <param name="paymentStatus">Optional payment status filter</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="searchTerm">Optional search term for order code or customer notes</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paged result of orders</returns>
        Task<PagedResult<Orders>> GetPagedOrdersAsync(
            Guid? accountId = null,
            Guid? shopId = null,
            OrderStatus? orderStatus = null,
            PaymentStatus? paymentStatus = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// Gets summary statistics for orders in a shop
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>Order statistics</returns>
        Task<OrderStatistics> GetOrderStatisticsAsync(Guid shopId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Orders>> GetShippedOrdersBeforeDateAsync(DateTime thresholdDate);
        Task<List<Orders>> GetOrdersByStatusAndCreatedBeforeAsync(OrderStatus status, DateTime cutoff);
        Task<List<Orders>> GetOrdersByStatusAndModifiedBeforeAsync(OrderStatus status, DateTime cutoff);
    }

    /// <summary>
    /// Statistics about orders
    /// </summary>
    public class OrderStatistics
    {
        /// <summary>
        /// Total number of orders
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// Number of orders by status
        /// </summary>
        public Dictionary<OrderStatus, int> OrdersByStatus { get; set; } = new Dictionary<OrderStatus, int>();

        /// <summary>
        /// Total revenue (sum of FinalAmount)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Total commission fees
        /// </summary>
        public decimal TotalCommissionFees { get; set; }

        /// <summary>
        /// Net revenue (Total Revenue - Total Commission Fees)
        /// </summary>
        public decimal NetRevenue { get; set; }

        /// <summary>
        /// Average order value
        /// </summary>
        public decimal AverageOrderValue { get; set; }

        /// <summary>
        /// Number of items sold
        /// </summary>
        public int TotalItemsSold { get; set; }

    }
}
