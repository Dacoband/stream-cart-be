namespace ChatBoxService.Application.DTOs
{
    public class ChatbotResponseDTO
    {
        public string BotResponse { get; set; } = string.Empty;
        public string Intent { get; set; } = string.Empty;
        public bool RequiresHumanSupport { get; set; }
        public List<SuggestedAction> SuggestedActions { get; set; } = new();
        public List<ProductSuggestion> ProductSuggestions { get; set; } = new();

        // ✅ THÊM ShopSuggestions property
        public List<ShopSuggestion> ShopSuggestions { get; set; } = new();

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public decimal ConfidenceScore { get; set; }
    }
}