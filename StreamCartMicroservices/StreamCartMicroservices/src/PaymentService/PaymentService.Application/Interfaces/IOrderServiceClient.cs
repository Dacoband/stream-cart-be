using System;
using System.Threading.Tasks;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Enums;

namespace PaymentService.Application.Interfaces
{
    /// <summary>
    /// Client for interacting with the Order Service API
    /// </summary>
    public interface IOrderServiceClient
    {
        /// <summary>
        /// Updates order payment status
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="paymentStatus">Status to update to</param>
        /// <returns>Task representing the async operation</returns>
        Task UpdateOrderPaymentStatusAsync(Guid orderId, PaymentStatus paymentStatus);

        /// <summary>
        /// Gets order details
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Order details if found</returns>
        Task<OrderDto?> GetOrderByIdAsync(Guid orderId);

        Task UpdateOrderStatusAsync(Guid orderId, OrderStatus orderStatus);
        Task<RefundRequestDto?> GetRefundRequestByIdAsync(Guid refundRequestId);

        /// <summary>
        /// ✅ Cập nhật trạng thái refund request
        /// </summary>
        Task<bool> UpdateRefundRequestStatusAsync(Guid refundRequestId, string status);
        Task<bool> UpdateRefundTransactionIdAsync(Guid refundRequestId, string transactionId);

    }
}