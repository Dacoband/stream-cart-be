using ChatBoxService.Application.DTOs;
using ChatBoxService.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ChatBoxService.Infrastructure.Services
{
    public interface ILivestreamOrderAIService
    {
        Task<OrderIntentResult> AnalyzeOrderIntentAsync(string message, Guid livestreamId, Guid userId);
        Task<OrderCreationResult> ProcessOrderFromMessageAsync(string message, Guid livestreamId, Guid userId);
    }

    public class LivestreamOrderAIService : ILivestreamOrderAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LivestreamOrderAIService> _logger;
        private readonly string _geminiApiKey;
        private readonly string _geminiApiUrl;
        private readonly ILivestreamServiceClient _livestreamServiceClient;
        private readonly IOrderServiceClient _orderServiceClient;
        private readonly IAddressServiceClient _addressServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly IHttpContextAccessor _httpContextAccessor; 


        public LivestreamOrderAIService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<LivestreamOrderAIService> logger,
            ILivestreamServiceClient livestreamServiceClient,
            IOrderServiceClient orderServiceClient,
            IAddressServiceClient addressServiceClient,
            IShopServiceClient shopServiceClient,IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _geminiApiKey = configuration["GEMINI_API_KEY"] ?? throw new InvalidOperationException("Gemini API Key not found");
            _geminiApiUrl = configuration["GEMINI_API_URL"] ?? "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
            _livestreamServiceClient = livestreamServiceClient;
            _orderServiceClient = orderServiceClient;
            _addressServiceClient = addressServiceClient;
            _shopServiceClient = shopServiceClient;
            _httpContextAccessor = httpContextAccessor;
        }
        private string? GetCurrentUserToken()
        {
            try
            {
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader?.StartsWith("Bearer ") == true)
                {
                    return authHeader.Substring(7); // Remove "Bearer " prefix
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get current user token");
                return null;
            }
        }

        public async Task<OrderIntentResult> AnalyzeOrderIntentAsync(string message, Guid livestreamId, Guid userId)
        {
            try
            {
                var systemPrompt = @"
Bạn là AI chuyên phân tích ý định đặt hàng trên livestream. Phân tích tin nhắn của viewer và trả về JSON với format sau:

{
  ""isOrderIntent"": true/false,
  ""extractedData"": {
    ""sku"": ""mã SKU sản phẩm"",
    ""quantity"": số_lượng,
    ""productName"": ""tên sản phẩm nếu có""
  },
  ""confidence"": 0.0-1.0,
  ""orderType"": ""direct_sku"" | ""product_name"" | ""mixed"",
  ""originalMessage"": ""tin nhắn gốc""
}

CÁC PATTERN ĐẶT HÀNG ĐƯỢC HỖ TRỢ:
1. ""Đặt ABC123 x2"" - SKU + số lượng với x
2. ""Mua 3 cái iPhone ABC123"" - Số lượng + tên + SKU  
3. ""Order ABC123 qty 5"" - SKU + quantity
4. ""Đặt hàng ABC123"" - chỉ SKU (quantity = 1)
5. ""ABC123*2"" - SKU + dấu * + số lượng 
6. ""LTBX*2"" - Format trực tiếp SKU*quantity
7. ""iPhone15Pro*3"" - Tên sản phẩm*số lượng
8. ""2 ABC123"" - Số lượng trước SKU
9. ""ABC1234"" - SKU kết thúc bằng số (cần phân biệt với quantity)

KHÔNG PHẢI ĐẶT HÀNG:
- Hỏi giá, thông tin sản phẩm
- Chat thường, emoji
- Khen ngợi sản phẩm mà không có ý định mua

Tin nhắn cần phân tích: """ + message + @"""";

                var response = await CallGeminiAPIAsync(systemPrompt, message);
                var result = JsonSerializer.Deserialize<OrderIntentResult>(response);

                return result ?? new OrderIntentResult
                {
                    IsOrderIntent = false,
                    Confidence = 0.0f,
                    OriginalMessage = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing order intent for message: {Message}", message);

                // Fallback: Simple regex pattern matching
                return AnalyzeOrderIntentFallback(message);
            }
        }

        public async Task<OrderCreationResult> ProcessOrderFromMessageAsync(string message, Guid livestreamId, Guid userId)
        {
            try
            {
                // 1. Phân tích ý định đặt hàng
                var intent = await AnalyzeOrderIntentAsync(message, livestreamId, userId);

                if (!intent.IsOrderIntent || intent.ExtractedData == null)
                {
                    return new OrderCreationResult
                    {
                        Success = false,
                        Message = "Tin nhắn không có ý định đặt hàng rõ ràng.",
                        OrderIntent = intent
                    };
                }

                // 2. Validate SKU và tìm sản phẩm trong livestream
                var product = await _livestreamServiceClient.GetProductBySkuAsync(livestreamId, intent.ExtractedData.Sku);
                if (product == null)
                {
                    return new OrderCreationResult
                    {
                        Success = false,
                        Message = $"Không tìm thấy sản phẩm với mã '{intent.ExtractedData.Sku}' trong livestream này.",
                        OrderIntent = intent
                    };
                }

                // 3. Kiểm tra tồn kho
                if (product.ProductStock < intent.ExtractedData.Quantity)
                {
                    return new OrderCreationResult
                    {
                        Success = false,
                        Message = $"Sản phẩm {product.ProductName} chỉ còn {product.ProductStock} sản phẩm. Bạn đặt {intent.ExtractedData.Quantity} sản phẩm.",
                        OrderIntent = intent,
                        // ✅ FIX: Convert to Application DTO
                        Product = ConvertToApplicationDTO(product)
                    };
                }

                // 4. Tạo StreamEvent trước (comment event)
                var streamEvent = await CreateStreamEventAsync(livestreamId, userId, message, product.Id);

                // 5. Cập nhật stock trước khi tạo order (reserve stock)
                var newStock = product.Stock - intent.ExtractedData.Quantity;
                var stockUpdateResult = await _livestreamServiceClient.UpdateProductStockAsync(
                    livestreamId,
                    product.ProductId,
                    product.VariantId,
                    newStock,
                    userId.ToString());

                if (!stockUpdateResult)
                {
                    return new OrderCreationResult
                    {
                        Success = false,
                        Message = "Không thể cập nhật tồn kho. Vui lòng thử lại.",
                        OrderIntent = intent,
                        Product = ConvertToApplicationDTO(product)
                    };
                }

                // 6. Tạo order với livestreamId và createdFromCommentId
                var orderResult = await CreateOrderAsync(livestreamId, userId, product, intent.ExtractedData.Quantity, streamEvent.Id);

                if (orderResult.Success)
                {
                    // 7. ✅ FIX: Check for null before passing to method
                    if (orderResult.OrderId.HasValue)
                    {
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromMinutes(10));
                            // ✅ FIX: Thêm livestreamId parameter
                            await CheckAndCancelUnpaidOrderAsync(orderResult.OrderId.Value, product, intent.ExtractedData.Quantity, livestreamId);
                        });
                    }

                    return new OrderCreationResult
                    {
                        Success = true,
                        Message = $"✅ Đặt hàng thành công!\n📦 Sản phẩm: {product.ProductName}\n🔢 Số lượng: {intent.ExtractedData.Quantity}\n💰 Tổng tiền: {product.Price * intent.ExtractedData.Quantity:N0} VND\n⏰ Vui lòng thanh toán trong vòng 10 phút.",
                        OrderIntent = intent,
                        // ✅ FIX: Convert to Application DTO
                        Product = ConvertToApplicationDTO(product),
                        OrderId = orderResult.OrderId,
                        StreamEventId = streamEvent.Id
                    };
                }
                else
                {
                    // Rollback stock nếu tạo order thất bại
                    var rollbackStock = newStock + intent.ExtractedData.Quantity; // Trả về stock ban đầu
                    await _livestreamServiceClient.UpdateProductStockAsync(
                        livestreamId,
                        product.ProductId,
                        product.VariantId,
                        rollbackStock,
                        "system-rollback");

                    return new OrderCreationResult
                    {
                        Success = false,
                        Message = "Có lỗi xảy ra khi tạo đơn hàng. Vui lòng thử lại.",
                        OrderIntent = intent,
                        // ✅ FIX: Convert to Application DTO
                        Product = ConvertToApplicationDTO(product)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order from message: {Message}", message);
                return new OrderCreationResult
                {
                    Success = false,
                    Message = "Có lỗi hệ thống. Vui lòng thử lại sau."
                };
            }
        }

        // ✅ FIX: Helper method to convert Infrastructure DTO to Application DTO
        private Application.DTOs.LivestreamProductDTO ConvertToApplicationDTO(Infrastructure.Services.LivestreamProductDTO infrastructureDto)
        {
            return new Application.DTOs.LivestreamProductDTO
            {
                Id = infrastructureDto.Id,
                ProductId = infrastructureDto.ProductId,
                VariantId = infrastructureDto.VariantId,
                SKU = infrastructureDto.SKU,
                ProductName = infrastructureDto.ProductName,
                Price = infrastructureDto.Price,
                Stock = infrastructureDto.Stock,
                ProductStock = infrastructureDto.ProductStock,
                ShopId = infrastructureDto.ShopId,
                ProductImageUrl = infrastructureDto.ProductImageUrl
            };
        }

        private OrderIntentResult AnalyzeOrderIntentFallback(string message)
        {
            var lowerMessage = message.ToLower();

            // ✅ ENHANCED Regex patterns for order detection - hỗ trợ nhiều format SKU hơn
            var orderPatterns = new[]
            {
        // ✅ Basic patterns with improved SKU matching
        @"đặt\s+([a-zA-Z0-9\-_*]+)\s*[x*]?\s*(\d+)?", // đặt LTBX*2, đặt ABC123 x2
        @"mua\s+(\d+)?\s*[^\s]*\s*([a-zA-Z0-9\-_*]+)", // mua 2 cái LTBX
        @"order\s+([a-zA-Z0-9\-_*]+)\s*qty?\s*(\d+)?", // order LTBX qty 2
        
        // ✅ NEW: Support for * as quantity separator
        @"([a-zA-Z0-9\-_*]+)\s*[*x]\s*(\d+)", // LTBX*2, ABC123*3, DEF456 x 2
        @"([a-zA-Z0-9\-_*]+)\*(\d+)", // LTBX*2 (direct format)
        
        // ✅ SKU with quantity without separator
        @"([a-zA-Z0-9\-_*]+)(\d+)$", // LTBX2 (SKU followed by number at end)
        
        // ✅ Traditional patterns
        @"sku:?\s*([a-zA-Z0-9\-_*]+)\s*(\d+)?", // SKU: LTBX 2
        @"(\d+)\s*([a-zA-Z0-9\-_*]+)", // 2 LTBX
    };

            foreach (var pattern in orderPatterns)
            {
                var match = Regex.Match(lowerMessage, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string sku;
                    int quantity;

                    // ✅ Special handling for different pattern groups
                    if (pattern.Contains(@"(\d+)\s*([a-zA-Z0-9\-_*]+)")) // Pattern: 2 LTBX
                    {
                        quantity = int.Parse(match.Groups[1].Value);
                        sku = match.Groups[2].Value;
                    }
                    else if (pattern.Contains(@"mua\s+(\d+)?")) // Pattern: mua 2 cái LTBX
                    {
                        var quantityStr = match.Groups[1].Value;
                        sku = match.Groups[2].Value;
                        quantity = string.IsNullOrEmpty(quantityStr) ? 1 : int.Parse(quantityStr);
                    }
                    else // Default pattern: SKU then quantity
                    {
                        sku = match.Groups[1].Value;
                        var quantityStr = match.Groups[2].Value;
                        quantity = string.IsNullOrEmpty(quantityStr) ? 1 : int.Parse(quantityStr);
                    }

                    // ✅ Clean up SKU - remove * if it's not part of actual SKU
                    if (sku.EndsWith("*") && !string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        sku = sku.TrimEnd('*');
                    }

                    _logger.LogInformation("✅ Fallback pattern matched - SKU: {SKU}, Quantity: {Quantity}, Pattern: {Pattern}",
                        sku, quantity, pattern);

                    return new OrderIntentResult
                    {
                        IsOrderIntent = true,
                        ExtractedData = new OrderExtractedData
                        {
                            Sku = sku.ToUpper(),
                            Quantity = quantity
                        },
                        Confidence = 0.8f,
                        OrderType = "direct_sku",
                        OriginalMessage = message
                    };
                }
            }

            _logger.LogWarning("❌ No pattern matched for message: {Message}", message);

            return new OrderIntentResult
            {
                IsOrderIntent = false,
                Confidence = 0.0f,
                OriginalMessage = message
            };
        }

        private async Task<StreamEventResult> CreateStreamEventAsync(Guid livestreamId, Guid userId, string message, Guid livestreamProductId)
        {
            // ✅ FIX: Call with correct parameters (livestreamId, userId, message, livestreamProductId)
            var result = await _livestreamServiceClient.CreateStreamEventAsync(livestreamId, userId, message, livestreamProductId);
            return result;
        }

        private async Task<CreateMultiOrderResult> CreateOrderAsync(Guid livestreamId, Guid userId, LivestreamProductDTO product, int quantity, Guid streamEventId)
        {
            var userToken = GetCurrentUserToken();
            // ✅ FIX: Sử dụng AddressServiceClient
            var userAddress = await _addressServiceClient.GetUserDefaultAddressAsync(userId);
            if (userAddress == null)
            {
                return new CreateMultiOrderResult
                {
                    Success = false,
                    Message = "Bạn cần cập nhật địa chỉ giao hàng trước khi đặt hàng."
                };
            }

            // ✅ FIX: Lấy thông tin shop từ ShopId trong livestream product
            var shopInfo = await _shopServiceClient.GetShopByIdAsync(product.ShopId);
            if (shopInfo == null)
            {
                return new CreateMultiOrderResult
                {
                    Success = false,
                    Message = "Không tìm thấy thông tin shop."
                };
            }

            // ✅ FIX: Lấy địa chỉ shop để làm FROM address
            var shopAddress = await _addressServiceClient.GetShopAddressAsync(product.ShopId);
            if (shopAddress == null)
            {
                return new CreateMultiOrderResult
                {
                    Success = false,
                    Message = "Không tìm thấy địa chỉ shop."
                };
            }
            var createOrderRequest = new CreateMultiOrderRequest
            {
                AccountId = userId,
                LivestreamId = livestreamId,
                CreatedFromCommentId = streamEventId, 
                PaymentMethod = "COD", 
                AddressId = userAddress.Id,
                OrdersByShop = new List<CreateOrderByShopDto>
                {
                    new CreateOrderByShopDto
                    {
                        ShopId = product.ShopId,
                        Items = new List<CreateOrderItemDto>
                        {
                            new CreateOrderItemDto
                            {
                                ProductId = Guid.Parse(product.ProductId),
                                VariantId = string.IsNullOrEmpty(product.VariantId) ? null : Guid.Parse(product.VariantId),
                                Quantity = quantity
                            }
                        },
                        ShippingFee = 0m, 
                        ShippingProviderId = Guid.NewGuid(), 
                        CustomerNotes = $"Đặt hàng từ livestream - SKU: {product.SKU} - Shop: {shopInfo.ShopName}"
                    }
                }
            };

            var orderResult = await _orderServiceClient.CreateMultiOrderAsync(createOrderRequest);
            return orderResult;
        }

        private async Task CheckAndCancelUnpaidOrderAsync(Guid orderId, LivestreamProductDTO product, int quantity, Guid livestreamId)
        {
            try
            {
                // Check order payment status
                var paymentStatus = await _orderServiceClient.GetOrderPaymentStatusAsync(orderId);
                if (paymentStatus == "Pending")
                {
                    // Cancel order
                    await _orderServiceClient.CancelOrderAsync(orderId, "Hủy tự động do không thanh toán trong 10 phút");

                    // ✅ FIX: Restore stock với đúng parameters
                    var restoreStock = product.Stock + quantity; // Trả về stock ban đầu
                    await _livestreamServiceClient.UpdateProductStockAsync(
                        livestreamId,
                        product.ProductId,
                        product.VariantId,
                        restoreStock,
                        "auto-restore");

                    _logger.LogInformation("Auto-cancelled unpaid order {OrderId} and restored stock", orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in auto-cancel order process for order {OrderId}", orderId);
            }
        }

        private async Task<string> CallGeminiAPIAsync(string systemPrompt, string userMessage)
        {
            // Implementation tương tự như GeminiChatbotService
            var client = _httpClientFactory.CreateClient();

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"{systemPrompt}\n\nTin nhắn: {userMessage}" }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.3, // Lower temperature for more consistent parsing
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 512
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var url = $"{_geminiApiUrl}?key={_geminiApiKey}";
            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Gemini API error: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            // ✅ FIX: Add GeminiResponse class
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

            return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "{}";
        }

        private async Task<UserAddressDTO> GetUserDefaultAddressAsync(Guid userId)
        {
            // Implementation để lấy địa chỉ mặc định của user
            // Call Address Service
            return null; // Placeholder
        }

        private async Task<ShopInfoDTO> GetShopInfoAsync(Guid shopId)
        {
            // Implementation để lấy thông tin shop
            // Call Shop Service  
            return null; // Placeholder
        }

        // ✅ FIX: Add missing GeminiResponse classes
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