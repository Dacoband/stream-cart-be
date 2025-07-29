using ChatBoxService.Application.DTOs;
using ChatBoxService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChatBoxService.Infrastructure.Services
{
    public class GeminiChatbotService : IGeminiChatbotService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<GeminiChatbotService> _logger;
        private readonly string _geminiApiKey;
        private readonly string _geminiApiUrl;

        public GeminiChatbotService(
            IHttpClientFactory httpClientFactory,
            IProductServiceClient productServiceClient,
            IShopServiceClient shopServiceClient,
            IConfiguration configuration,
            ILogger<GeminiChatbotService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _productServiceClient = productServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
            _geminiApiKey = configuration["GEMINI_API_KEY"] ?? configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key is not configured");
            _geminiApiUrl = configuration["GEMINI_API_URL"] ?? "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";
        }
        public async Task<string> GenerateResponseAsync(string customerMessage, Guid shopId, Guid? productId = null)
        {
            try
            {
                // Get shop information
                var shopInfo = await _shopServiceClient.GetShopByIdAsync(shopId);
                var shopContext = shopInfo != null ? $"Cửa hàng: {shopInfo.ShopName}" : "Cửa hàng không xác định";

                // Get product information if provided
                string productContext = "";
                if (productId.HasValue)
                {
                    var product = await _productServiceClient.GetProductByIdAsync(productId.Value);
                    if (product != null)
                    {
                        productContext = $"\nSản phẩm: {product.ProductName}\nGiá: {product.Price:N0} VND\nMô tả: {product.Description}";
                    }
                }

                // Create system prompt for friendly customer service
                var systemPrompt = $@"Bạn là một trợ lý ảo thân thiện và chuyên nghiệp của {shopContext}. 
Nhiệm vụ của bạn là:
1. Trả lời các câu hỏi của khách hàng một cách thân thiện và hữu ích
2. Cung cấp thông tin chính xác về sản phẩm và dịch vụ
3. Hướng dẫn khách hàng trong quá trình mua sắm
4. Luôn giữ thái độ lịch sự và nhiệt tình
5. Nếu không biết thông tin chính xác, hãy thành thật và đề xuất liên hệ nhân viên hỗ trợ

{productContext}

Hãy trả lời câu hỏi của khách hàng bằng tiếng Việt, giọng điệu thân thiện và chuyên nghiệp.";

                var response = await CallGeminiAPIAsync(systemPrompt, customerMessage);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating response for customer message: {Message}", customerMessage);
                return "Xin lỗi, tôi đang gặp một chút trục trặc. Vui lòng thử lại sau hoặc liên hệ với nhân viên hỗ trợ để được giúp đỡ tốt nhất.";
            }
        }
        public async Task<string> GenerateProductResponseAsync(string customerMessage, Guid productId)
        {
            try
            {
                var product = await _productServiceClient.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return "Xin lỗi, tôi không tìm thấy thông tin về sản phẩm này. Vui lòng kiểm tra lại hoặc liên hệ nhân viên hỗ trợ.";
                }

                var systemPrompt = $@"Bạn là chuyên gia tư vấn sản phẩm thân thiện và am hiểu.
Thông tin sản phẩm:
- Tên: {product.ProductName}
- Giá: {product.Price:N0} VND
- Mô tả: {product.Description}
- Còn hàng: {(product.Stock > 0 ? $"{product.Stock} sản phẩm" : "Hết hàng")}

Hãy trả lời câu hỏi của khách hàng về sản phẩm này một cách chi tiết, thân thiện và hữu ích. 
Nếu khách hàng quan tâm đến việc mua, hãy khuyến khích họ đặt hàng.
Luôn sử dụng tiếng Việt và giọng điệu nhiệt tình, chuyên nghiệp.";

                var response = await CallGeminiAPIAsync(systemPrompt, customerMessage);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating product response for product {ProductId}", productId);
                return "Xin lỗi, tôi không thể cung cấp thông tin về sản phẩm này lúc này. Vui lòng thử lại sau.";
            }
        }
        public async Task<ChatbotIntent> AnalyzeMessageIntentAsync(string customerMessage)
        {
            try
            {
                var systemPrompt = @"Phân tích ý định của tin nhắn khách hàng và trả về JSON với format:
{
  ""intent"": ""greeting|product_inquiry|price_question|availability|complaint|order_status|general_question"",
  ""category"": ""customer_service|product_info|order_management|technical_support"",
  ""keywords"": [""từ khóa quan trọng""],
  ""requiresProductInfo"": true/false,
  ""requiresShopInfo"": true/false,
  ""confidence"": 0.8
}";

                var response = await CallGeminiAPIAsync(systemPrompt, customerMessage);

                // Try to parse JSON response
                try
                {
                    var intent = JsonSerializer.Deserialize<ChatbotIntent>(response);
                    return intent ?? new ChatbotIntent { Intent = "general_question", Confidence = 0.5m };
                }
                catch
                {
                    // Fallback if JSON parsing fails
                    return new ChatbotIntent
                    {
                        Intent = "general_question",
                        Category = "customer_service",
                        Confidence = 0.5m,
                        Keywords = customerMessage.Split(' ').Take(3).ToList()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing message intent: {Message}", customerMessage);
                return new ChatbotIntent { Intent = "general_question", Confidence = 0.3m };
            }
        }
        private async Task<string> CallGeminiAPIAsync(string systemPrompt, string userMessage)
        {
            var client = _httpClientFactory.CreateClient();

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"{systemPrompt}\n\nKhách hàng hỏi: {userMessage}" }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 1024,
                    stopSequences = new string[] { }
                },
                safetySettings = new[]
                {
                    new
                    {
                        category = "HARM_CATEGORY_HARASSMENT",
                        threshold = "BLOCK_MEDIUM_AND_ABOVE"
                    },
                    new
                    {
                        category = "HARM_CATEGORY_HATE_SPEECH",
                        threshold = "BLOCK_MEDIUM_AND_ABOVE"
                    }
                }
            };
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"{_geminiApiUrl}?key={_geminiApiKey}";
            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Gemini API error: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

            return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ??
                   "Xin lỗi, tôi không thể trả lời câu hỏi này lúc này. Vui lòng liên hệ nhân viên hỗ trợ.";
        }

        private class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public List<Candidate>? Candidates { get; set; }
        }

        private class Candidate
        {
            [JsonPropertyName("content")]
            public Content? Content { get; set; }
        }

        private class Content
        {
            [JsonPropertyName("parts")]
            public List<Part>? Parts { get; set; }
        }
        private class Part
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }
}
