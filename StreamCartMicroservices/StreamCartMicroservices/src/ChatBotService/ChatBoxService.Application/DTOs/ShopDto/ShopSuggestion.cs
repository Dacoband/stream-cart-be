namespace ChatBoxService.Application.DTOs
{
    public class ShopSuggestion
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
        public decimal Rating { get; set; }
        public int ProductCount { get; set; }
        public string? Location { get; set; }
        public string ReasonForSuggestion { get; set; } = string.Empty;
        public decimal RelevanceScore { get; set; }
    }
}