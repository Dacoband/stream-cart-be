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
        [Required]
        [StringLength(100, ErrorMessage = "Bank name must not exceed 100 characters")]
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// ✅ Số tài khoản ngân hàng để nhận tiền hoàn trả
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Bank number must not exceed 50 characters")]
        public string BankNumber { get; set; } = string.Empty;
    
}

    public class RefundItemDto
    {
        [Required]
        public Guid OrderItemId { get; set; }
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? ImageUrl { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Bank name must not exceed 100 characters")]
        public string BankName { get; set; } = string.Empty;
        [Required]
        [StringLength(50, ErrorMessage = "Bank number must not exceed 50 characters")]
        public string BankNumber { get; set; } = string.Empty;
    }
}