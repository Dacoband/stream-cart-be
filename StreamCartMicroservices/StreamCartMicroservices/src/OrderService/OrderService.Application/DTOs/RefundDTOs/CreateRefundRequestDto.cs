using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.RefundDTOs
{
    public class CreateRefundRequestDto
    {
        /// <summary>
        /// ID of the order to refund
        /// </summary>
        [Required]
        public Guid OrderId { get; set; }

        /// <summary>
        /// List of order item IDs to refund with reasons
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one order item must be specified")]
        public List<RefundItemDto> RefundItems { get; set; } = new();

        /// <summary>
        /// Shipping fee for return (optional)
        /// </summary>
       // public decimal ShippingFee { get; set; } = 0;
    }

    public class RefundItemDto
    {
        /// <summary>
        /// Order item ID to refund
        /// </summary>
        [Required]
        public Guid OrderItemId { get; set; }

        /// <summary>
        /// Reason for refund
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Optional image URL for evidence
        /// </summary>
        [MaxLength(1000)]
        public string? ImageUrl { get; set; }
    }
}