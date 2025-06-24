using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces.IServices
{
    public interface IOrderService
    {
        /// <summary>
        /// Creates a new order
        /// </summary>
        /// <param name="createOrderDto">Order creation data</param>
        /// <returns>Created order data</returns>
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);

        /// <summary>
        /// Gets an order by its ID
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Order or null if not found</returns>
        Task<OrderDto> GetOrderByIdAsync(Guid orderId);

        /// <summary>
        /// Gets an order by its order code
        /// </summary>
        /// <param name="orderCode">Order code</param>
        /// <returns>Order or null if not found</returns>
        Task<OrderDto> GetOrderByCodeAsync(string orderCode);

        /// <summary>
        /// Gets orders for a specific account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>Paged result of orders</returns>
        Task<PagedResult<OrderDto>> GetOrdersByAccountIdAsync(Guid accountId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets orders for a specific shop
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>Paged result of orders</returns>
        Task<PagedResult<OrderDto>> GetOrdersByShopIdAsync(Guid shopId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Searches for orders based on various filters
        /// </summary>
        /// <param name="searchParams">Search parameters</param>
        /// <returns>Paged result of orders</returns>
        Task<PagedResult<OrderDto>> SearchOrdersAsync(OrderSearchParamsDto searchParams);

        /// <summary>
        /// Updates an order's status
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="newStatus">New order status</param>
        /// <param name="modifiedBy">User who made the change</param>
        /// <returns>Updated order</returns>
        Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, string modifiedBy);

        /// <summary>
        /// Updates an order's payment status
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="newStatus">New payment status</param>
        /// <param name="modifiedBy">User who made the change</param>
        /// <returns>Updated order</returns>
        Task<OrderDto> UpdatePaymentStatusAsync(Guid orderId, PaymentStatus newStatus, string modifiedBy);

        /// <summary>
        /// Updates an order's tracking code
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="trackingCode">New tracking code</param>
        /// <param name="modifiedBy">User who made the change</param>
        /// <returns>Updated order</returns>
        Task<OrderDto> UpdateTrackingCodeAsync(Guid orderId, string trackingCode, string modifiedBy);

        /// <summary>
        /// Updates an order's shipping information
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="shippingAddress">New shipping address</param>
        /// <param name="shippingMethod">New shipping method (optional)</param>
        /// <param name="shippingFee">New shipping fee (optional)</param>
        /// <param name="modifiedBy">User who made the change</param>
        /// <returns>Updated order</returns>
        Task<OrderDto> UpdateShippingInfoAsync(Guid orderId, ShippingAddressDto shippingAddress, string shippingMethod = null, decimal? shippingFee = null, string modifiedBy = null);

        /// <summary>
        /// Cancels an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="cancelReason">Reason for cancellation (optional)</param>
        /// <param name="cancelledBy">User who cancelled the order</param>
        /// <returns>Cancelled order</returns>
        Task<OrderDto> CancelOrderAsync(Guid orderId, string cancelReason, string cancelledBy);

        /// <summary>
        /// Gets order statistics for a shop
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="startDate">Start date for statistics (optional)</param>
        /// <param name="endDate">End date for statistics (optional)</param>
        /// <returns>Order statistics</returns>
        Task<OrderStatisticsDto> GetOrderStatisticsAsync(Guid shopId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Validates an order before creation
        /// </summary>
        /// <param name="createOrderDto">Order creation data</param>
        /// <returns>Validation result</returns>
        Task<(bool IsValid, string ErrorMessage)> ValidateOrderAsync(CreateOrderDto createOrderDto);
    }
}