using System;

namespace LivestreamService.Application.DTOs
{
    public class LivestreamProductDetailDTO
    {
        public Guid Id { get; set; }
        public Guid LivestreamId { get; set; }
        public string? ProductId { get; set; }
        public string? VariantId { get; set; }
        public Guid? FlashSaleId { get; set; }
        public bool IsPin { get; set; }
        public decimal OriginalPrice { get; set; } 
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        // Product details from ProductService
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public string? ProductImageUrl { get; set; }
        public string? VariantName { get; set; }
        public decimal BasePrice { get; set; }
        public int ProductSoldQuantity { get; set; }
        public bool ProductIsActive { get; set; }

        // Flash Sale info (if exists)
        

        // Shop info
        public Guid ShopId { get; set; }
        public string? ShopName { get; set; }
    }
}