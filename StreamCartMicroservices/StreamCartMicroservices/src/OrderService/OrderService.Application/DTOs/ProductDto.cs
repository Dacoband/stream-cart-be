using System;

namespace OrderService.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public Guid ShopId { get; set; }
    }
}
