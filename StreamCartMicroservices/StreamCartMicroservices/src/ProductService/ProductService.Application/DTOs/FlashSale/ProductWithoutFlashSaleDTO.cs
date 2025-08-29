using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.FlashSale
{
    public class ProductWithoutFlashSaleDTO
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public decimal BasePrice { get; set; }
        public int StockQuantity { get; set; }
        public string? ProductImageUrl { get; set; }
        public List<ProductVariantWithoutFlashSaleDTO>? Variants { get; set; }
    }

    public class ProductVariantWithoutFlashSaleDTO
    {
        public Guid Id { get; set; }
        public string? SKU { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? VariantName { get; set; }
    }
}