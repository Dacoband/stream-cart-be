using ShopService.Domain.Enums;

namespace ShopService.Application.DTOs.Voucher
{
    /// <summary>
    /// DTO cho response voucher khả dụng của customer
    /// </summary>
    public class CustomerVoucherResponseDto
    {
        public CustomerVoucherDto Voucher { get; set; } = new();
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string DiscountMessage { get; set; } = string.Empty;
    }
    public class CustomerVoucherDto
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public VoucherType Type { get; set; }
        public string TypeDisplayName => Type switch
        {
            VoucherType.Percentage => "Giảm theo %",
            VoucherType.FixedAmount => "Giảm cố định",
            _ => "Không xác định"
        };
        public decimal Value { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RemainingQuantity { get; set; }

        // Shop info
        public string? ShopName { get; set; }
        public string? ShopImageUrl { get; set; }
        public double HoursRemaining => Math.Max(0, (EndDate - DateTime.UtcNow).TotalHours);
        public bool IsExpiringSoon => HoursRemaining <= 24 && HoursRemaining > 0;
    }
}