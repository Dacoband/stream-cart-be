using System;
using System.Threading.Tasks;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Interfaces
{
    /// <summary>
    /// Service for sending payment notifications
    /// </summary>
    public interface IPaymentNotificationService
    {
        /// <summary>
        /// Sends payment confirmation notification
        /// </summary>
        /// <param name="payment">Payment details</param>
        /// <returns>Task representing the async operation</returns>
        Task SendPaymentConfirmationAsync(PaymentDto payment);

        /// <summary>
        /// Sends payment failure notification
        /// </summary>
        /// <param name="payment">Payment details</param>
        /// <param name="reason">Failure reason</param>
        /// <returns>Task representing the async operation</returns>
        Task SendPaymentFailureAsync(PaymentDto payment, string reason);

        /// <summary>
        /// Sends refund notification
        /// </summary>
        /// <param name="payment">Payment details</param>
        /// <returns>Task representing the async operation</returns>
        Task SendRefundNotificationAsync(PaymentDto payment);
    }
}