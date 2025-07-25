using System;

namespace OrderService.Application.DTOs
{
    public class VariantDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string[] Attributes { get; set; } = Array.Empty<string>();
    }
}