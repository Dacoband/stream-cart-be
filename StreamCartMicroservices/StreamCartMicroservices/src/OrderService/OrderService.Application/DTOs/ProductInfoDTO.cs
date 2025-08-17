namespace OrderService.Application.DTOs
{
    public class ProductInfoDTO
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public Guid? ShopId { get; set; }
        public string? ShopName { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}