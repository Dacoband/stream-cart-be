namespace ChatBoxService.Application.DTOs
{
    public class ProductSuggestion
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public decimal RelevanceScore { get; set; }
        public string ShopName { get; set; } = string.Empty;

        // ✅ THÊM các properties cần thiết
        public Guid ShopId { get; set; }
        public string ReasonForSuggestion { get; set; } = string.Empty;
    }
}