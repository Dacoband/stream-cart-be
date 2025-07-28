using ShopService.Domain.Enums;
using System;

namespace ShopService.Application.DTOs.Voucher
{
    public class ShopVoucherDto
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public VoucherType Type { get; set; }
        public string TypeDisplayName => Type == VoucherType.Percentage ? "Phần trăm" : "Số tiền cố định";
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
}