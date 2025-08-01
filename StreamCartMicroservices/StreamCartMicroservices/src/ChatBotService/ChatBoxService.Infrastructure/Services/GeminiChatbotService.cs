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
        private readonly IChatHistoryService _chatHistoryService;


        public GeminiChatbotService(
            IHttpClientFactory httpClientFactory,
            IProductServiceClient productServiceClient,
            IShopServiceClient shopServiceClient,
            IConfiguration configuration,
            ILogger<GeminiChatbotService> logger,
            IChatHistoryService chatHistoryService)
        {
            _httpClientFactory = httpClientFactory;
            _productServiceClient = productServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
            _geminiApiKey = configuration["GEMINI_API_KEY"] ?? configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key is not configured");
            _geminiApiUrl = configuration["GEMINI_API_URL"] ?? configuration["Gemini:ApiUrl"] ??
    "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
            _chatHistoryService = chatHistoryService;
        }

        public async Task<string> GenerateResponseAsync(string customerMessage, Guid shopId, Guid userID, Guid? productId = null)
        {
            try
            {
                var conversation = await _chatHistoryService.GetOrCreateConversationAsync(userID, shopId);

                var conversationContext = await _chatHistoryService.GetConversationContextAsync(userID, shopId, 5);

                // Get shop information
                var shopInfo = await _shopServiceClient.GetShopByIdAsync(shopId);
                var shopContext = shopInfo != null ? $"Cửa hàng: {shopInfo.ShopName}" : "Cửa hàng không xác định";

                // ✅ THÔNG MINH: Phân tích tin nhắn TRƯỚC để quyết định cần lấy gì
                var intent = await AnalyzeMessageIntentAsync(customerMessage);
                _logger.LogInformation("Analyzed intent: {Intent} for message: {Message}", intent.Intent, customerMessage);
                await _chatHistoryService.AddMessageToConversationAsync(
            conversation.ConversationId,
            customerMessage,
            "User",
            intent.Intent,
            intent.Confidence);

                string productContext = "";

                // ✅ Nếu có ProductId cụ thể, ưu tiên lấy sản phẩm đó
                if (productId.HasValue)
                {
                    var product = await _productServiceClient.GetProductByIdAsync(productId.Value);
                    if (product != null)
                    {
                        productContext = $@"
📦 SẢN PHẨM ĐANG ĐƯỢC QUAN TÂM:
• Tên: {product.ProductName}
• Giá: {product.BasePrice:N0} VND
• Mô tả: {product.Description}
• Tồn kho: {(product.StockQuantity > 0 ? $"Còn {product.StockQuantity} sản phẩm" : "⚠️ Hết hàng")}";
                    }
                }
                // ✅ Dựa vào intent để tự động lấy thông tin sản phẩm
                else if (IsProductInquiry(intent.Intent, customerMessage))
                {
                    productContext = await BuildProductContextAsync(shopId, customerMessage, intent);
                }

                // Enhanced system prompt với context thông minh
                var systemPrompt = $@"Bạn là trợ lý ảo thân thiện và chuyên nghiệp của {shopContext} trên nền tảng StreamCart. 

NGUYÊN TẮC TRẢ LỜI:
1. Luôn trả lời bằng tiếng Việt tự nhiên, thân thiện như người Việt Nam
2. Hiểu và phản hồi các câu chào hỏi, lời cảm ơn theo văn hóa Việt
3. Sử dụng emoji phù hợp để tạo không khí thân thiện 😊
4. Gọi khách hàng bằng ""anh/chị"" hoặc ""bạn"" một cách lịch sự
5. ✅ DỰA VÀO THÔNG TIN SẢN PHẨM THỰC TẾ để tư vấn chính xác

CÁC CÂU TRẢ LỜI MẪU CHO CÁC TÌNH HUỐNG THƯỜNG GẶP:

🔸 Chào hỏi (hello, hi, chào, xin chào):
- ""Xin chào anh/chị! Chào mừng bạn đến với {shopContext} 😊 Tôi có thể hỗ trợ gì cho bạn hôm nay?""

🔸 Cảm ơn (cảm ơn, thanks, thank you):
- ""Dạ không có gì ạ! Rất vui được hỗ trợ anh/chị 😊 Còn gì khác tôi có thể giúp không ạ?""

🔸 Hỏi về sản phẩm chung (có sản phẩm gì, bán gì, hàng gì):
- Sử dụng thông tin sản phẩm bên dưới để giới thiệu cụ thể
- Khuyến khích khách hàng xem chi tiết và đặt hàng

🔸 Hỏi sản phẩm cụ thể:
- Trả lời dựa trên thông tin sản phẩm có sẵn
- Nếu không có, gợi ý sản phẩm tương tự

🔸 Hỏi giá cả:
- Đưa ra thông tin giá chính xác từ database
- So sánh và tư vấn về tính cạnh tranh

🔸 Hỏi về tồn kho:
- Dựa vào thông tin tồn kho để trả lời cụ thể
- Khuyến khích đặt hàng sớm nếu sản phẩm sắp hết

🔸 Không hiểu câu hỏi:
- ""Xin lỗi, tôi chưa hiểu rõ ý bạn. Bạn có thể nói rõ hơn không ạ? Hoặc anh/chị có thể hỏi về sản phẩm cụ thể nào của shop!""

🔸 Lời khen ngợi:
- ""Cảm ơn anh/chị đã tin tưởng shop! Chúng tôi luôn cố gắng mang đến sản phẩm tốt nhất 😊""

{productContext}

NHIỆM VỤ CHÍNH:
1. ✅ Tư vấn sản phẩm dựa trên danh sách có sẵn
2. ✅ Giới thiệu sản phẩm phù hợp với nhu cầu khách hàng  
3. ✅ Đưa ra thông tin giá cả và tồn kho chính xác
4. ✅ Khuyến khích mua sắm một cách tự nhiên
5. ✅ Hướng dẫn khách hàng cách đặt hàng
6. Tạo cảm giác thân thiện, đáng tin cậy

✅ QUAN TRỌNG: HÃY SỬ DỤNG THÔNG TIN SẢN PHẨM THỰC TẾ ở trên để trả lời chính xác!

HÃY TRẢ LỜI CÂU HỎI CỦA KHÁCH HÀNG:";

                var response = await CallGeminiAPIAsync(systemPrompt, customerMessage);
                await _chatHistoryService.AddMessageToConversationAsync(
            conversation.ConversationId,
            response,
            "Bot");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating response for customer message: {Message}", customerMessage);
                return "Xin lỗi anh/chị, tôi đang gặp một chút trục trặc kỹ thuật 😅 Vui lòng thử lại sau hoặc liên hệ với nhân viên hỗ trợ để được giúp đỡ tốt nhất nhé!";
            }
        }

        public async Task<string> GenerateProductResponseAsync(string customerMessage, Guid productId)
        {
            try
            {
                var product = await _productServiceClient.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return "Xin lỗi anh/chị, tôi không tìm thấy thông tin về sản phẩm này. Có thể sản phẩm đã hết hàng hoặc không còn kinh doanh. Vui lòng liên hệ nhân viên để được tư vấn các sản phẩm tương tự nhé! 😊";
                }

                var systemPrompt = $@"Bạn là chuyên gia tư vấn sản phẩm chuyên nghiệp và thân thiện tại StreamCart.

THÔNG TIN SẢN PHẨM CẦN TƯ VẤN:
📦 Tên: {product.ProductName}
💰 Giá: {product.BasePrice:N0} VND
📝 Mô tả: {product.Description}
📊 Tình trạng: {(product.StockQuantity > 0 ? $"Còn {product.StockQuantity} sản phẩm" : "⚠️ Đang hết hàng")}

CÁCH TRẢ LỜI CHUYÊN NGHIỆP:

🔸 Với câu hỏi về giá:
- ""Sản phẩm này có giá {product.BasePrice:N0} VND anh/chị ạ. Đây là mức giá rất cạnh tranh cho chất lượng sản phẩm này!""

🔸 Với câu hỏi về chất lượng:
- Dựa vào mô tả để đưa ra đánh giá chi tiết về ưu điểm
- Cam kết về chất lượng và chế độ bảo hành

🔸 Với câu hỏi về tình trạng hàng:
- Nếu còn hàng: ""Sản phẩm hiện đang có sẵn, còn {product.StockQuantity} sản phẩm. Anh/chị nên đặt hàng sớm để đảm bảo có hàng nhé!""
- Nếu hết hàng: ""Sản phẩm này hiện đang hết hàng. Tôi sẽ thông báo ngay khi có hàng về, hoặc anh/chị có thể xem các sản phẩm tương tự khác!""

🔸 Với câu hỏi so sánh:
- ""Sản phẩm này có những ưu điểm vượt trội như [liệt kê ưu điểm]. So với các sản phẩm cùng phân khúc thì rất đáng giá!""

🔸 Khi khách hàng quan tâm mua:
- ""Tuyệt vời! Để đặt hàng, anh/chị chỉ cần click vào nút 'Mua ngay' hoặc 'Thêm vào giỏ hàng'. Shop sẽ giao hàng nhanh chóng và đảm bảo chất lượng nhé! 😊""

🔸 Về giao hàng và bảo hành:
- ""Shop có chính sách giao hàng nhanh và đổi trả linh hoạt. Sản phẩm được bảo hành đầy đủ theo quy định!""

HÃY TRẢ LỜI CHUYÊN NGHIỆP VÀ THUYẾT PHỤC:";

                var response = await CallGeminiAPIAsync(systemPrompt, customerMessage);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating product response for product {ProductId}", productId);
                return "Xin lỗi anh/chị, tôi không thể lấy thông tin sản phẩm lúc này. Vui lòng thử lại sau hoặc liên hệ nhân viên để được tư vấn trực tiếp nhé! 😊";
            }
        }

        public async Task<ChatbotIntent> AnalyzeMessageIntentAsync(string customerMessage)
        {
            try
            {
                var systemPrompt = @"Phân tích ý định tin nhắn tiếng Việt của khách hàng và trả về JSON chính xác:

CÁC LOẠI Ý ĐỊNH (intent):
- ""greeting"": chào hỏi (xin chào, hello, hi, chào bạn, chào shop)
- ""thanks"": cảm ơn (cảm ơn, thanks, thank you, cám ơn)
- ""product_inquiry"": hỏi về sản phẩm (có sản phẩm gì, bán gì, hàng gì, sản phẩm nào, mặt hàng)
- ""price_question"": hỏi về giá (giá bao nhiêu, bao tiền, giá cả, giá thế nào)
- ""availability"": hỏi tình trạng hàng (còn hàng không, có sẵn không, hết hàng, tồn kho)
- ""quality_question"": hỏi về chất lượng (chất lượng thế nào, có tốt không, review, đánh giá)
- ""shipping_question"": hỏi về giao hàng (giao hàng, ship, vận chuyển, delivery)
- ""complaint"": khiếu nại, phản ánh (không hài lòng, tệ, kém, phàn nàn)
- ""compliment"": khen ngợi (tốt, đẹp, chất lượng, ổn, hay)
- ""order_status"": hỏi về đơn hàng (đơn hàng, order, trạng thái đơn)
- ""search_product"": tìm kiếm sản phẩm cụ thể (tìm, search, có [tên sản phẩm] không)
- ""general_question"": câu hỏi chung

DANH MỤC (category):
- ""customer_service"": dịch vụ khách hàng
- ""product_info"": thông tin sản phẩm  
- ""order_management"": quản lý đơn hàng
- ""technical_support"": hỗ trợ kỹ thuật

Format JSON trả về:
{
  ""intent"": ""loại_ý_định"",
  ""category"": ""danh_mục"",
  ""keywords"": [""từ_khóa_quan_trọng""],
  ""requiresProductInfo"": true/false,
  ""requiresShopInfo"": true/false,
  ""confidence"": 0.8
}

Phân tích tin nhắn: """ + customerMessage + @"""";

                var response = await CallGeminiAPIAsync(systemPrompt, customerMessage);

                // Try to parse JSON response
                try
                {
                    var intent = JsonSerializer.Deserialize<ChatbotIntent>(response);
                    return intent ?? new ChatbotIntent { Intent = "general_question", Confidence = 0.5m };
                }
                catch
                {
                    // Enhanced fallback with better Vietnamese understanding
                    var message = customerMessage.ToLower();

                    if (message.Contains("xin chào") || message.Contains("hello") || message.Contains("hi") || message.Contains("chào"))
                    {
                        return new ChatbotIntent
                        {
                            Intent = "greeting",
                            Category = "customer_service",
                            Confidence = 0.9m,
                            Keywords = new List<string> { "chào hỏi" },
                            RequiresProductInfo = false,
                            RequiresShopInfo = true
                        };
                    }
                    else if (message.Contains("cảm ơn") || message.Contains("thanks") || message.Contains("cám ơn"))
                    {
                        return new ChatbotIntent
                        {
                            Intent = "thanks",
                            Category = "customer_service",
                            Confidence = 0.9m,
                            Keywords = new List<string> { "cảm ơn" },
                            RequiresProductInfo = false,
                            RequiresShopInfo = false
                        };
                    }
                    else if (message.Contains("sản phẩm") || message.Contains("hàng") || message.Contains("bán gì") || message.Contains("có gì"))
                    {
                        return new ChatbotIntent
                        {
                            Intent = "product_inquiry",
                            Category = "product_info",
                            Confidence = 0.8m,
                            Keywords = new List<string> { "sản phẩm", "hàng hóa" },
                            RequiresProductInfo = true,
                            RequiresShopInfo = true
                        };
                    }
                    else if (message.Contains("giá") || message.Contains("bao nhiêu") || message.Contains("tiền"))
                    {
                        return new ChatbotIntent
                        {
                            Intent = "price_question",
                            Category = "product_info",
                            Confidence = 0.8m,
                            Keywords = new List<string> { "giá", "tiền" },
                            RequiresProductInfo = true,
                            RequiresShopInfo = false
                        };
                    }

                    // Default fallback
                    return new ChatbotIntent
                    {
                        Intent = "general_question",
                        Category = "customer_service",
                        Confidence = 0.5m,
                        Keywords = customerMessage.Split(' ').Take(3).ToList(),
                        RequiresProductInfo = false,
                        RequiresShopInfo = true
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing message intent: {Message}", customerMessage);
                return new ChatbotIntent { Intent = "general_question", Confidence = 0.3m };
            }
        }

        // ✅ XÂY DỰNG PRODUCT CONTEXT DỰA TRÊN PHÂN TÍCH TIN NHẮN
        private async Task<string> BuildProductContextAsync(Guid shopId, string customerMessage, ChatbotIntent intent)
        {
            try
            {
                // Trường hợp 1: Khách hỏi về sản phẩm cụ thể (có tên sản phẩm trong tin nhắn)
                var productName = ExtractProductNameFromMessage(customerMessage);
                if (!string.IsNullOrEmpty(productName))
                {
                    var specificProduct = await SearchSpecificProductAsync(shopId, productName);
                    if (specificProduct != null)
                    {
                        return $@"
📦 SẢN PHẨM ĐƯỢC HỎI: {specificProduct.ProductName}
💰 Giá: {specificProduct.BasePrice:N0} VND
📝 Mô tả: {specificProduct.Description}
📊 Tình trạng: {(specificProduct.StockQuantity > 0 ? $"Còn {specificProduct.StockQuantity} sản phẩm" : "⚠️ Hết hàng")}";
                    }
                    else
                    {
                        // Không tìm thấy sản phẩm cụ thể, hiển thị sản phẩm tương tự
                        var similarProducts = await _productServiceClient.GetProductsByShopIdAsync(shopId, activeOnly: true);
                        if (similarProducts.Any())
                        {
                            return $@"
❌ Không tìm thấy sản phẩm ""{productName}""
📦 CÁC SẢN PHẨM TƯƠNG TỰ TRONG SHOP:
{string.Join("\n", similarProducts.Take(3).Select(p => $"• {p.ProductName} - {p.BasePrice:N0} VND"))}";
                        }
                        else
                        {
                            return "\n📦 Shop hiện chưa có sản phẩm nào được đăng bán.";
                        }
                    }
                }
                // Trường hợp 2: Khách hỏi về sản phẩm chung chung (có sản phẩm gì, bán gì...)
                else if (IsGeneralProductInquiry(customerMessage))
                {
                    var allProducts = await _productServiceClient.GetProductsByShopIdAsync(shopId, activeOnly: true);
                    if (allProducts.Any())
                    {
                        var featuredProducts = allProducts.Take(6).ToList();
                        var context = "\n📦 CÁC SẢN PHẨM CỦA SHOP:\n";

                        foreach (var product in featuredProducts)
                        {
                            context += $"• {product.ProductName} - {product.BasePrice:N0} VND";
                            context += product.StockQuantity > 0 ? $" (Còn {product.StockQuantity})\n" : " (Hết hàng)\n";
                        }

                        if (allProducts.Count > 6)
                        {
                            context += $"... và {allProducts.Count - 6} sản phẩm khác\n";
                        }

                        context += $"\n🔢 Tổng cộng: {allProducts.Count} sản phẩm đang có sẵn";
                        return context;
                    }
                    else
                    {
                        return "\n📦 Shop hiện chưa có sản phẩm nào được đăng bán.";
                    }
                }
                // Trường hợp 3: Hỏi về giá cả chung
                else if (IsPriceInquiry(customerMessage))
                {
                    var products = await _productServiceClient.GetProductsByShopIdAsync(shopId, activeOnly: true);
                    if (products.Any())
                    {
                        var minPrice = products.Min(p => p.BasePrice);
                        var maxPrice = products.Max(p => p.BasePrice);
                        var avgPrice = products.Average(p => p.BasePrice);

                        return $@"
💰 THÔNG TIN GIÁ CẢ:
• Giá thấp nhất: {minPrice:N0} VND
• Giá cao nhất: {maxPrice:N0} VND  
• Giá trung bình: {avgPrice:N0} VND
• Tổng số sản phẩm: {products.Count}";
                    }
                    else
                    {
                        return "\n📦 Shop hiện chưa có sản phẩm nào để báo giá.";
                    }
                }
                // Trường hợp 4: Hỏi về tồn kho
                else if (IsStockInquiry(customerMessage))
                {
                    var products = await _productServiceClient.GetProductsByShopIdAsync(shopId, activeOnly: true);
                    if (products.Any())
                    {
                        var inStock = products.Where(p => p.StockQuantity > 0).ToList();
                        var outOfStock = products.Where(p => p.StockQuantity == 0).ToList();

                        return $@"
📊 TÌNH TRẠNG TỒN KHO:
• Sản phẩm còn hàng: {inStock.Count}
• Sản phẩm hết hàng: {outOfStock.Count}
• Tổng tồn kho: {products.Sum(p => p.StockQuantity)} sản phẩm";
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building product context for shop {ShopId}", shopId);
                return "";
            }
        }

        // ✅ Helper methods để phân tích tin nhắn
        private static bool IsProductInquiry(string intent, string message)
        {
            var productIntents = new[] { "product_inquiry", "price_question", "availability", "search_product", "quality_question" };
            var shopPatterns = new[] { "có shop nào", "shop nào", "shop bán", "cửa hàng bán" };

            // Kiểm tra các pattern về shop bán sản phẩm
            if (shopPatterns.Any(p => message.ToLower().Contains(p)))
            {
                return true;
            }

            return productIntents.Contains(intent) ||
                   message.ToLower().Contains("sản phẩm") ||
                   message.ToLower().Contains("hàng") ||
                   message.ToLower().Contains("bán gì") ||
                   message.ToLower().Contains("có gì");
        }

        private static bool IsGeneralProductInquiry(string message)
        {
            var generalQuestions = new[] { "có sản phẩm gì", "bán gì", "hàng gì", "có gì", "mặt hàng", "danh mục", "sản phẩm nào" };
            return generalQuestions.Any(q => message.ToLower().Contains(q));
        }

        private static bool IsPriceInquiry(string message)
        {
            var priceQuestions = new[] { "giá", "bao nhiêu", "tiền", "chi phí", "giá cả" };
            return priceQuestions.Any(q => message.ToLower().Contains(q)) &&
                   !message.ToLower().Contains("sản phẩm cụ thể"); // Không phải hỏi giá sản phẩm cụ thể
        }

        private static bool IsStockInquiry(string message)
        {
            var stockQuestions = new[] { "còn hàng", "tồn kho", "có sẵn", "hết hàng", "số lượng" };
            return stockQuestions.Any(q => message.ToLower().Contains(q));
        }

        private static string ExtractProductNameFromMessage(string message)
        {
            // Logic đơn giản để extract tên sản phẩm từ tin nhắn
            var lowerMessage = message.ToLower();

            // Tìm pattern: "có [tên sản phẩm] không", "[tên sản phẩm] giá bao nhiêu", etc.
            var patterns = new[]
            {
        @"có (.+?) không",
        @"(.+?) giá bao nhiêu",
        @"(.+?) còn hàng",
        @"tìm (.+)",
        @"(.+?) chất lượng",
        @"(.+?) có tốt",
        @"shop có (.+)",
        // Thêm pattern mới để bắt mẫu câu "có shop nào bán iphone"
        @"shop nào bán (.+)",
        @"có shop nào bán (.+)"
    };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(lowerMessage, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    var productName = match.Groups[1].Value.Trim();
                    // Loại bỏ các từ không cần thiết
                    var stopWords = new[] { "sản phẩm", "hàng", "mặt hàng", "cái", "chiếc", "của shop", "này", "không", "gì" };
                    foreach (var stopWord in stopWords)
                    {
                        productName = productName.Replace(stopWord, "").Trim();
                    }

                    if (!string.IsNullOrEmpty(productName) && productName.Length > 2)
                    {
                        return productName;
                    }
                }
            }

            return string.Empty;
        }

        private async Task<ProductDto?> SearchSpecificProductAsync(Guid shopId, string productName)
        {
            try
            {
                // Lấy tất cả sản phẩm của shop và tìm kiếm theo tên
                var products = await _productServiceClient.GetProductsByShopIdAsync(shopId, activeOnly: true);

                if (!products.Any())
                    return null;

                // Tìm sản phẩm có tên gần giống nhất (exact match)
                var exactMatch = products.FirstOrDefault(p =>
                    p.ProductName?.Contains(productName, StringComparison.OrdinalIgnoreCase) == true);

                if (exactMatch != null)
                    return exactMatch;

                // Tìm sản phẩm có từ khóa tương tự
                var similarMatch = products.FirstOrDefault(p =>
                    productName.Split(' ').Any(word =>
                        p.ProductName?.Contains(word, StringComparison.OrdinalIgnoreCase) == true && word.Length > 2));

                return similarMatch;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for product: {ProductName}", productName);
                return null;
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
                    temperature = 0.8, // Tăng temperature để có phản hồi tự nhiên hơn
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
                   "Xin lỗi anh/chị, tôi không thể trả lời câu hỏi này lúc này. Vui lòng liên hệ nhân viên hỗ trợ để được giúp đỡ tốt nhất nhé! 😊";
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