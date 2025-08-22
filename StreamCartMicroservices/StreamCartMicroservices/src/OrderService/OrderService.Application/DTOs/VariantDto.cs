using System;

namespace OrderService.Application.DTOs
{
    public class VariantDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        //public string VariantName { get; set; } = string.Empty;
        //public string Name { get; set; } = string.Empty; 

        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }    
        public decimal? Weight { get; set; }
    }
}