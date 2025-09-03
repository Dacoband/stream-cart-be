using System;

namespace PaymentService.Application.DTOs
{
    /// <summary>
    /// DTO for refund callback data
    /// </summary>
    public class RefundCallbackDto
    {
        /// <summary>
        /// Transaction ID from payment provider
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Refund amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Status: "success" or "failed"
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Full content from callback
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Transfer type: "in" or "out"
        /// </summary>
        public string TransferType { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when callback was received
        /// </summary>
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}