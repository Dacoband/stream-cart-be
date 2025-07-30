namespace ChatBoxService.Application.DTOs
{
    /// <summary>
    /// DTO for Product information from Product Service
    /// </summary>
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public bool HasVariant { get; set; }
        public int QuantitySold { get; set; }
        public Guid ShopId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public bool HasPrimaryImage { get; set; }

        // Alias properties for compatibility with older code
        public decimal Price => FinalPrice;
        public int Stock => StockQuantity;
    }
}