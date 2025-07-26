using System;

namespace ShopService.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public bool HasVariant { get; set; }
        public int QuantitySold { get; set; }
        public Guid? ShopId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool HasPrimaryImage { get; set; }
        public string? PrimaryImageUrl { get; set; }
    }
}