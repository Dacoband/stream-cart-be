using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs
{
    public class VoucherValidationDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public ShopVoucherDto? Voucher { get; set; }
    }
    public class ShopVoucherDto
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AvailableQuantity { get; set; }
        public int UsedQuantity { get; set; }
        public int RemainingQuantity => AvailableQuantity - UsedQuantity;
        public bool IsActive { get; set; }
        public bool IsValid { get; set; }
        public bool IsExpired => DateTime.UtcNow > EndDate;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }

        // Shop information
        public string? ShopName { get; set; }
    }
    public class VoucherApplicationDto
    {
        public bool IsApplied { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public Guid? VoucherId { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public DateTime? AppliedAt { get; set; }
    }
}
