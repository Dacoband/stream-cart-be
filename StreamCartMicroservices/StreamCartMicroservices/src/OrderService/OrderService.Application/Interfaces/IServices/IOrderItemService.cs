using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces.IServices
{
    public interface IOrderItemService
    {
        /// <summary>
        /// Gets an order item by ID
        /// </summary>
        /// <param name="orderItemId">Order item ID</param>
        /// <returns>Order item or null if not found</returns>
        Task<OrderItemDto> GetOrderItemByIdAsync(Guid orderItemId);

        /// <summary>
        /// Gets all items for a specific order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Collection of order items</returns>
        Task<IEnumerable<OrderItemDto>> GetOrderItemsByOrderIdAsync(Guid orderId);

        /// <summary>
        /// Creates a new order item
        /// </summary>
        /// <param name="createOrderItemDto">Order item creation data</param>
        /// <returns>Created order item</returns>
        Task<OrderItemDto> CreateOrderItemAsync(CreateOrderItemDto createOrderItemDto);

        /// <summary>
        /// Updates an existing order item
        /// </summary>
        /// <param name="orderItemId">Order item ID</param>
        /// <param name="quantity">New quantity</param>
        /// <param name="unitPrice">New unit price</param>
        /// <param name="discountAmount">New discount amount</param>
        /// <param name="notes">New notes</param>
        /// <param name="modifiedBy">User who made the change</param>
        /// <returns>Updated order item</returns>
        Task<OrderItemDto> UpdateOrderItemAsync(Guid orderItemId, int quantity, decimal unitPrice, decimal discountAmount, string notes, string modifiedBy);

        /// <summary>
        /// Removes an order item
        /// </summary>
        /// <param name="orderItemId">Order item ID</param>
        /// <param name="removedBy">User who removed the item</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> RemoveOrderItemAsync(Guid orderItemId, string removedBy);

        /// <summary>
        /// Gets sales statistics for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="shopId">Optional shop ID filter</param>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>Product sales statistics</returns>
        Task<ProductSalesStatisticsDto> GetProductSalesStatisticsAsync(Guid productId, Guid? shopId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets sales statistics for all products in a shop
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <param name="topProductsLimit">Optional limit for top selling products</param>
        /// <returns>Collection of product sales statistics</returns>
        Task<IEnumerable<ProductSalesStatisticsDto>> GetShopSalesStatisticsAsync(Guid shopId, DateTime? startDate = null, DateTime? endDate = null, int? topProductsLimit = null);

        /// <summary>
        /// Applies a refund to an order item
        /// </summary>
        /// <param name="orderItemId">Order item ID</param>
        /// <param name="refundRequestId">Refund request ID</param>
        /// <param name="modifiedBy">User who processed the refund</param>
        /// <returns>Updated order item</returns>
        Task<OrderItemDto> ApplyRefundAsync(Guid orderItemId, Guid refundRequestId, string modifiedBy);

        /// <summary>
        /// Validates an order item before creation
        /// </summary>
        /// <param name="createOrderItemDto">Order item creation data</param>
        /// <returns>Validation result</returns>
        Task<(bool IsValid, string ErrorMessage)> ValidateOrderItemAsync(CreateOrderItemDto createOrderItemDto);
    }
}