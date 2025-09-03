using System;

namespace ProductService.Application.DTOs.Variants
{
    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? SKU { get; set; }
        public decimal Price { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public decimal? FinalPrice { get; set; } =0; 
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}