namespace ProductService.Application.DTOs.Products
{
    public class ProductSearchItemDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public int QuantitySold { get; set; }
        public decimal DiscountPercentage { get; set; }
        public bool IsOnSale { get; set; }
        public bool InStock { get; set; }
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string ShopLocation { get; set; } = string.Empty;
        public decimal ShopRating { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string HighlightedName { get; set; } = string.Empty;
    }
}