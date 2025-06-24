using OrderService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces.IRepositories
{
    public interface IOrderItemRepository : IGenericRepository<OrderItem>
    {
        /// <summary>
        /// Gets all items for a specific order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Collection of order items</returns>
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId);

        /// <summary>
        /// Gets all items for a specific product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Collection of order items</returns>
        Task<IEnumerable<OrderItem>> GetByProductIdAsync(Guid productId);

        /// <summary>
        /// Gets all items for a specific product variant
        /// </summary>
        /// <param name="variantId">Variant ID</param>
        /// <returns>Collection of order items</returns>
        Task<IEnumerable<OrderItem>> GetByVariantIdAsync(Guid variantId);

        /// <summary>
        /// Gets all items linked to a specific refund request
        /// </summary>
        /// <param name="refundRequestId">Refund request ID</param>
        /// <returns>Collection of order items</returns>
        Task<IEnumerable<OrderItem>> GetByRefundRequestIdAsync(Guid refundRequestId);

        /// <summary>
        /// Gets product sales statistics (quantity sold, revenue)
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>Product sales statistics</returns>
        Task<ProductSalesStatistics> GetProductSalesStatisticsAsync(
            Guid productId, 
            DateTime? startDate = null, 
            DateTime? endDate = null);
        /// <summary>
        /// Gets sales statistics for all products in a shop
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="topProductsLimit">Optional limit for top-selling products</param>
        /// <returns>Collection of product sales statistics for the shop</returns>
        Task<IEnumerable<ProductSalesStatistics>> GetShopSalesStatisticsAsync(
            Guid shopId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? topProductsLimit = null);

        /// <summary>
        /// Gets all items for multiple orders
        /// </summary>
        /// <param name="orderIds">Order IDs</param>
        /// <returns>Collection of order items</returns>
        Task<IEnumerable<OrderItem>> GetByOrderIdsAsync(IEnumerable<Guid> orderIds);

        /// <summary>
        /// Gets all items with paging and filter options
        /// </summary>
        /// <param name="orderId">Optional order ID filter</param>
        /// <param name="productId">Optional product ID filter</param>
        /// <param name="variantId">Optional variant ID filter</param>
        /// <param name="minQuantity">Optional minimum quantity filter</param>
        /// <param name="minUnitPrice">Optional minimum unit price filter</param>
        /// <param name="maxUnitPrice">Optional maximum unit price filter</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paged result of order items</returns>
        Task<PagedResult<OrderItem>> GetPagedOrderItemsAsync(
            Guid? orderId = null,
            Guid? productId = null,
            Guid? variantId = null,
            int? minQuantity = null,
            decimal? minUnitPrice = null,
            decimal? maxUnitPrice = null,
            int pageNumber = 1,
            int pageSize = 10);
    }

    /// <summary>
    /// Statistics about product sales
    /// </summary>
    public class ProductSalesStatistics
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Total quantity sold
        /// </summary>
        public int TotalQuantitySold { get; set; }

        /// <summary>
        /// Total revenue generated (Quantity * UnitPrice - DiscountAmount)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Average unit price
        /// </summary>
        public decimal AverageUnitPrice { get; set; }

        /// <summary>
        /// Average discount per item
        /// </summary>
        public decimal AverageDiscount { get; set; }

        /// <summary>
        /// Dictionary of variant IDs and their quantities sold
        /// </summary>
        public Dictionary<Guid, int> VariantQuantities { get; set; } = new Dictionary<Guid, int>();

        /// <summary>
        /// Number of orders containing this product
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// Number of refund requests for this product
        /// </summary>
        public int RefundCount { get; set; }
    }
}
