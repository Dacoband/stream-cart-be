using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.RefundDTOs
{
    /// <summary>
    /// DTO for updating refund transaction ID
    /// </summary>
    public class UpdateRefundTransactionDto
    {
        /// <summary>
        /// Refund request ID
        /// </summary>
        [Required]
        public Guid RefundRequestId { get; set; }

        /// <summary>
        /// Transaction ID from payment provider (SePay)
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Transaction ID must not exceed 100 characters")]
        public string TransactionId { get; set; } = string.Empty;
    }
}