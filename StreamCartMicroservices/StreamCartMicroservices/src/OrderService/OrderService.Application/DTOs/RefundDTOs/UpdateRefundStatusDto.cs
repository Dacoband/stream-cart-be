using OrderService.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.RefundDTOs
{
    public class UpdateRefundStatusDto
    {
        /// <summary>
        /// Refund request ID
        /// </summary>
        [Required]
        public Guid RefundRequestId { get; set; }

        /// <summary>
        /// New status for the refund request
        /// </summary>
        [Required]
        public RefundStatus NewStatus { get; set; }

        /// <summary>
        /// User who is updating the status
        /// </summary>
        [Required]
        public string ModifiedBy { get; set; } = string.Empty;

        /// <summary>
        /// Optional tracking code when status is Packed or OnDelivery
        /// </summary>
        public string? TrackingCode { get; set; }
    }
}