using System;

namespace ProductService.Application.DTOs.Variants
{
    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
    }

    public class CreateProductVariantDto
    {
        public Guid ProductId { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public int Stock { get; set; }
    }

    public class UpdateProductVariantDto
    {
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public int Stock { get; set; }
    }

    public class UpdateStockDto
    {
        public int Quantity { get; set; }
    }

    public class UpdatePriceDto
    {
        public decimal Price { get; set; }
        public decimal? FlashSalePrice { get; set; }
    }

    public class BulkUpdateStockDto
    {
        public List<VariantStockUpdate> StockUpdates { get; set; } = new List<VariantStockUpdate>();
    }

    public class VariantStockUpdate
    {
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
    }
}