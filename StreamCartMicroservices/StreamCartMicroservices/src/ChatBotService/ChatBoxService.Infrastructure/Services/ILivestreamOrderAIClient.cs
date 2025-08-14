using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatBoxService.Infrastructure.Services
{
    public interface ILivestreamOrderAIClient
    {
        Task<OrderAnalysisResponse> AnalyzeOrderIntentAsync(string message, Guid livestreamId, Guid userId);
    }

    public class LivestreamOrderAIClient : ILivestreamOrderAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LivestreamOrderAIClient> _logger;
        private readonly string _aiServiceUrl;

        public LivestreamOrderAIClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<LivestreamOrderAIClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _aiServiceUrl = configuration["AI_ORDER_SERVICE_URL"] ?? "http://localhost:8080";
        }

        public async Task<OrderAnalysisResponse> AnalyzeOrderIntentAsync(string message, Guid livestreamId, Guid userId)
        {
            try
            {
                var request = new
                {
                    message = message,
                    livestream_id = livestreamId.ToString(),
                    user_id = userId.ToString(),
                    context = new { timestamp = DateTime.UtcNow }
                };

                var response = await _httpClient.PostAsJsonAsync($"{_aiServiceUrl}/analyze-order-intent", request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<OrderAnalysisResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });

                    return result ?? new OrderAnalysisResponse { IsOrderIntent = false, Message = "AI service error" };
                }

                // Fallback nếu AI service down
                return FallbackAnalysis(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling AI order service");
                return FallbackAnalysis(message);
            }
        }

        private OrderAnalysisResponse FallbackAnalysis(string message)
        {
            // Simple regex fallback
            var lowerMessage = message.ToLower();

            if (lowerMessage.Contains("đặt") || lowerMessage.Contains("mua") || lowerMessage.Contains("order"))
            {
                return new OrderAnalysisResponse
                {
                    IsOrderIntent = true,
                    Confidence = 0.5f,
                    Message = "Phát hiện ý định đặt hàng (fallback mode)"
                };
            }

            return new OrderAnalysisResponse
            {
                IsOrderIntent = false,
                Confidence = 0.0f,
                Message = "Không phát hiện ý định đặt hàng"
            };
        }
    }

    public class OrderAnalysisResponse
    {
        [JsonPropertyName("is_order_intent")]
        public bool IsOrderIntent { get; set; }

        [JsonPropertyName("extracted_data")]
        public ExtractedOrderData? ExtractedData { get; set; }

        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }

        [JsonPropertyName("order_type")]
        public string? OrderType { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("processing_time_ms")]
        public int ProcessingTimeMs { get; set; }
    }

    public class ExtractedOrderData
    {
        [JsonPropertyName("sku")]
        public string Sku { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;

        [JsonPropertyName("product_name")]
        public string? ProductName { get; set; }
    }
}