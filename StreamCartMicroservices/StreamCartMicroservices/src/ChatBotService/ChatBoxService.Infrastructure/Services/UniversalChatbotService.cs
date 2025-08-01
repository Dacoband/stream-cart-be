using ChatBoxService.Application.DTOs;
using ChatBoxService.Application.DTOs.ShopDto;
using ChatBoxService.Application.Interfaces;
using ChatBoxService.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChatBoxService.Infrastructure.Services
{
    public class UniversalChatbotService : IUniversalChatbotService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<UniversalChatbotService> _logger;
        private readonly string _geminiApiKey;
        private readonly string _geminiApiUrl;
        private readonly IChatHistoryService _chatHistoryService;


        // ✅ Circuit breaker state
        private static int _consecutiveFailures = 0;
        private static DateTime _lastFailureTime = DateTime.MinValue;
        private const int MAX_FAILURES = 3;
        private const int CIRCUIT_BREAKER_TIMEOUT_MINUTES = 5;


        private readonly ICachingService _cachingService;

        public UniversalChatbotService(
            IHttpClientFactory httpClientFactory,
            IProductServiceClient productServiceClient,
            IShopServiceClient shopServiceClient,
            IConfiguration configuration,
            ILogger<UniversalChatbotService> logger,
            IChatHistoryService chatHistoryService,ICachingService cachingService)
        {
            _httpClientFactory = httpClientFactory;
            _productServiceClient = productServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
            _cachingService = cachingService;
            _geminiApiKey = configuration["GEMINI_API_KEY"] ?? configuration["Gemini:ApiKey"] ??
                throw new InvalidOperationException("Gemini API Key is not configured");
            _geminiApiUrl = configuration["GEMINI_API_URL"] ?? configuration["Gemini:ApiUrl"] ??
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";

            _chatHistoryService = chatHistoryService;
        }

        public async Task<ChatbotResponseDTO> GenerateUniversalResponseAsync(string customerMessage, Guid userId)
        {
            try
            {
                var lowerMessage = customerMessage.ToLower().Trim();
                if ((lowerMessage.Contains("có shop nào bán") || lowerMessage.Contains("shop nào bán"))
                    && lowerMessage.Contains("iphone"))
                {
                    _logger.LogInformation("🎯 DETECTED EXACT PATTERN: có shop nào bán iPhone");

                    // Tìm shops bán iPhone
                    var shops = await SearchShopsUniversalAsync(new List<string> { "iphone" });

                    string response;
                    if (shops?.Any() == true)
                    {
                        response = "🏪 **Các shop bán iPhone trên StreamCart:**\n\n";
                        foreach (var shop in shops.Take(5))
                        {
                            response += $"• **{shop.ShopName}** - {shop.TotalProducts} sản phẩm\n";
                        }
                        response += "\n⭐ Đây là những shop uy tín trên StreamCart có bán iPhone. Anh/chị có thể truy cập vào shop để xem các mẫu iPhone và so sánh giá!";
                    }
                    else
                    {
                        response = "Hiện tại StreamCart có một số shop bán iPhone, nhưng tôi không thể tìm thấy thông tin cụ thể. Anh/chị có thể vào mục danh mục Điện thoại để tìm kiếm các sản phẩm iPhone nhé!";
                    }

                    var directResponse = new ChatbotResponseDTO
                    {
                        BotResponse = response,
                        Intent = "shop_search",
                        ConfidenceScore = 0.99m,
                        RequiresHumanSupport = false,
                        GeneratedAt = DateTime.UtcNow,
                        SuggestedActions = GenerateUniversalSuggestedActions(new ChatbotIntent { Intent = "shop_search" })
                    };
                    // Thêm shop suggestions nếu có
                    if (shops?.Any() == true)
                    {
                        directResponse.ShopSuggestions = shops.Take(5).Select(s => new ShopSuggestion
                        {
                            ShopId = s.Id,
                            ShopName = s.ShopName,
                            ProductCount = s.TotalProducts,
                            Rating = 4.5m,
                            Location = s.Address ?? "Việt Nam",
                            LogoUrl = s.LogoUrl,
                            Description = s.Description,
                            ReasonForSuggestion = "Cửa hàng bán iPhone"
                        }).ToList();
                    }

                    // Lưu conversation
                    if (userId != Guid.Empty)
                    {
                        await SaveUniversalConversationAsync(userId, customerMessage, directResponse);
                    }

                    return directResponse;
                }
                _logger.LogInformation("🤖 Processing universal chatbot request for user {UserId}: {Message}",
                    userId, customerMessage);

                // 1. Phân tích ý định tin nhắn - SỬ DỤNG OFFLINE PRIORITY
                var intent = await AnalyzeUniversalIntentAsync(customerMessage);
                _logger.LogInformation("🎯 Analyzed universal intent: {Intent} with confidence {Confidence}",
                    intent.Intent, intent.Confidence);

                // ✅ KIỂM TRA INTENT SHOP_SEARCH TRƯỚC TIÊN
                if (intent.Intent == "shop_search" && intent.Confidence >= 0.8m)
                {
                    _logger.LogInformation("🏪 Processing high-confidence shop search intent: {Message}", customerMessage);

                    // Xây dựng context dựa trên intent shop_search
                    var shopSearchContext = await BuildUniversalContextAsync(customerMessage, intent, userId);

                    // Xử lý fallback hoặc AI response cho shop_search
                    string aiResponse;
                    if (ShouldUseFallbackResponse(intent, shopSearchContext))
                    {
                        _logger.LogInformation("💡 Using smart fallback for shop search");
                        aiResponse = GetFallbackResponse(intent, shopSearchContext);
                    }
                    else if (IsCircuitBreakerOpen())
                    {
                        _logger.LogWarning("⚠️ Circuit breaker is OPEN - using fallback for shop search");
                        aiResponse = GetFallbackResponse(intent, shopSearchContext);
                    }
                    else
                    {
                        var systemPrompt = GenerateUniversalSystemPrompt(shopSearchContext, intent);
                        aiResponse = await CallGeminiAPIAsync(systemPrompt, customerMessage);
                    }

                    // Xây dựng response với suggestions từ database thật
                    var shopSearchResponse = await BuildChatbotResponseAsync(aiResponse, intent, shopSearchContext);

                    // Lưu lịch sử chat
                    if (userId != Guid.Empty)
                    {
                        await SaveUniversalConversationAsync(userId, customerMessage, shopSearchResponse);
                    }

                    _logger.LogInformation("✅ Generated shop search response with {ShopCount} shops",
                        shopSearchResponse.ShopSuggestions?.Count ?? 0);

                    return shopSearchResponse;
                }

                // ✓ FAST PATH cho các intent đơn giản
                if (intent.Intent == "greeting" || intent.Intent == "thanks")
                {
                    // Tạo fast response không cần context
                    string responseText = intent.Intent == "greeting"
                        ? ResponseTemplates.GetRandomResponse("greeting")
                        : ResponseTemplates.GetRandomResponse("thanks");

                    var fastResponse = new ChatbotResponseDTO
                    {
                        BotResponse = responseText,
                        Intent = intent.Intent,
                        ConfidenceScore = intent.Confidence,
                        RequiresHumanSupport = false,
                        SuggestedActions = GenerateUniversalSuggestedActions(intent),
                        GeneratedAt = DateTime.UtcNow
                    };

                    // Lưu conversation nếu cần
                    if (userId != Guid.Empty)
                    {
                        await SaveUniversalConversationAsync(userId, customerMessage, fastResponse);
                    }

                    return fastResponse;
                }

                // ✓ Kiểm tra FAQ matches - CHỈ KHI KHÔNG PHẢI INTENT CẦN XỬ LÝ ĐẶC BIỆT
                if (intent.Intent != "product_search" && intent.Confidence < 0.8m)
                {
                    bool isGoodFaqMatch;
                    var faqResponse = LocalTrainingData.FindBestMatchingFAQ(customerMessage, out isGoodFaqMatch);

                    if (isGoodFaqMatch)
                    {
                        _logger.LogInformation("📚 Found good FAQ match for message");

                        var faqFastResponse = new ChatbotResponseDTO
                        {
                            BotResponse = faqResponse + "\n\n" + LocalTrainingData.GetRandomShoppingTip(),
                            Intent = "general_question",
                            ConfidenceScore = 0.95m,
                            RequiresHumanSupport = false,
                            SuggestedActions = GenerateUniversalSuggestedActions(new ChatbotIntent { Intent = "general_question" }),
                            GeneratedAt = DateTime.UtcNow
                        };

                        if (userId != Guid.Empty)
                        {
                            await SaveUniversalConversationAsync(userId, customerMessage, faqFastResponse);
                        }

                        return faqFastResponse;
                    }
                }

                // 2. Xây dựng context dựa trên intent - THỰC SỰ GỌI API
                var context = await BuildUniversalContextAsync(customerMessage, intent, userId);

                // 3. **KIỂM TRA CÓ THỂ DÙNG FALLBACK KHÔNG?**
                string aiResponseText;

                // ✅ ƯU TIÊN FALLBACK cho các intent đơn giản
                if (ShouldUseFallbackResponse(intent, context))
                {
                    _logger.LogInformation("💡 Using smart fallback response for intent: {Intent}", intent.Intent);
                    aiResponseText = GetFallbackResponse(intent, context);
                }
                else if (IsCircuitBreakerOpen())
                {
                    _logger.LogWarning("⚠️ Circuit breaker is OPEN - using fallback response");
                    aiResponseText = GetFallbackResponse(intent, context);
                }
                else
                {
                    // 4. CHỈ GỌI AI khi thực sự cần thiết
                    try
                    {
                        var systemPrompt = GenerateUniversalSystemPrompt(context, intent);
                        aiResponseText = await CallGeminiAPIAsync(systemPrompt, customerMessage);
                    }
                    catch (HttpRequestException ex) when (ex.Message.Contains("ServiceUnavailable") || ex.Message.Contains("overloaded"))
                    {
                        _logger.LogWarning("⚠️ Gemini API overloaded - using fallback response for intent: {Intent}", intent.Intent);
                        aiResponseText = GetFallbackResponse(intent, context);
                    }
                }

                // 5. Xây dựng response với suggestions THẬT từ database
                var chatbotResponse = await BuildChatbotResponseAsync(aiResponseText, intent, context);

                // 6. Lưu lịch sử chat
                if (userId != Guid.Empty)
                {
                    await SaveUniversalConversationAsync(userId, customerMessage, chatbotResponse);
                }

                _logger.LogInformation("✅ Generated universal response with {ShopCount} shops and {ProductCount} products",
                    chatbotResponse.ShopSuggestions?.Count ?? 0, chatbotResponse.ProductSuggestions?.Count ?? 0);

                return chatbotResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating universal response for user {UserId}", userId);

                // ✅ Fallback cuối cùng với context database thật
                var fallbackIntent = CreateFallbackIntent(customerMessage);
                var fallbackContext = await BuildUniversalContextAsync(customerMessage, fallbackIntent, userId);
                var fallbackResponse = GetFallbackResponse(fallbackIntent, fallbackContext);

                return await BuildChatbotResponseAsync(fallbackResponse, fallbackIntent, fallbackContext);
            }
        }

        // ✅ Method mới: Quyết định có nên dùng fallback không
        private bool ShouldUseFallbackResponse(ChatbotIntent intent, UniversalContext context)
        {
            // Những intent này có thể dùng fallback hoàn toàn mà không cần AI
            var simpleFallbackIntents = new[] {
        "greeting",
        "thanks",
        "general_question"
    };

            if (simpleFallbackIntents.Contains(intent.Intent))
            {
                _logger.LogInformation("📝 Intent {Intent} can use simple fallback", intent.Intent);
                return true;
            }

            // Nếu có đủ dữ liệu từ database, có thể dùng fallback
            if (intent.Intent == "product_search" && context.SuggestedProducts?.Count >= 3)
            {
                _logger.LogInformation("📦 Product search has {Count} products - using fallback", context.SuggestedProducts.Count);
                return true;
            }

            if (intent.Intent == "shop_search" && context.SuggestedShops?.Count >= 2)
            {
                _logger.LogInformation("🏪 Shop search has {Count} shops - using fallback", context.SuggestedShops.Count);
                return true;
            }

            if (intent.Intent == "price_inquiry" && context.PriceRanges?.Count > 0)
            {
                _logger.LogInformation("💰 Price inquiry has price data - using fallback");
                return true;
            }

            return false;
        }
        // ✅ NEW: Method tìm kiếm shop theo tên
        private async Task<List<ShopDto>> SearchShopsByNameExactAsync(string shopName)
        {
            try
            {
                if (string.IsNullOrEmpty(shopName))
                    return new List<ShopDto>();

                var cacheKey = $"shop_exact_name_{shopName.ToLower()}";

                return await _cachingService.GetOrCreateAsync(cacheKey, async () =>
                {
                    _logger.LogInformation("🔍 Searching shops by exact name: {ShopName}", shopName);

                    // Gọi API tìm kiếm shop theo tên
                    var shops = await _shopServiceClient.SearchShopsByNameAsync(shopName);

                    // Tìm shop có tên chính xác nhất
                    var exactMatch = shops.FirstOrDefault(s =>
                        s.ShopName.Equals(shopName, StringComparison.OrdinalIgnoreCase));

                    // Nếu không có match chính xác, lấy shop có tên gần đúng nhất
                    if (exactMatch == null)
                    {
                        exactMatch = shops.FirstOrDefault(s =>
                            s.ShopName.Contains(shopName, StringComparison.OrdinalIgnoreCase) ||
                            shopName.Contains(s.ShopName, StringComparison.OrdinalIgnoreCase));
                    }

                    // Trả về danh sách chỉ có shop match hoặc empty list
                    return exactMatch != null ? new List<ShopDto> { exactMatch } : new List<ShopDto>();
                }, TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error searching shop by exact name: {ShopName}", shopName);
                return new List<ShopDto>();
            }
        }
        // ✅ NEW: Method trích xuất tên shop từ tin nhắn
        private string ExtractShopNameFromMessage(string message)
        {
            var lowerMessage = message.ToLower();

            // Các pattern phổ biến khi hỏi về sản phẩm của shop
            var shopNamePatterns = new[]
            {
        @"(sản\s*phẩm|hàng\s*hóa|đồ|mặt\s*hàng)\s*(của|ở|tại)\s*(shop|cửa\s*hàng)\s*([^?.,]+)",
        @"(shop|cửa\s*hàng)\s*([^?.,]+?)\s*(có|bán|cung\s*cấp|gì|hàng\s*gì)"
    };

            foreach (var pattern in shopNamePatterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(lowerMessage, pattern);
                if (match.Success)
                {
                    // Lấy group chứa tên shop (group 4 hoặc 2 tùy pattern)
                    string shopName = match.Groups.Count > 4 ? match.Groups[4].Value.Trim() : match.Groups[2].Value.Trim();

                    if (!string.IsNullOrEmpty(shopName))
                    {
                        _logger.LogInformation("🏪 Extracted shop name: {ShopName}", shopName);
                        return shopName;
                    }
                }
            }

            return string.Empty;
        }

        public async Task<ChatbotIntent> AnalyzeUniversalIntentAsync(string customerMessage)
        {
            try
            {
                // ✅ **LUÔN THỬ OFFLINE TRƯỚC** - nhanh và không tốn API calls
                var offlineIntent = AnalyzeIntentOffline(customerMessage);

                // Nếu offline analysis cho kết quả tốt (confidence >= 0.8), dùng luôn
                if (offlineIntent.Confidence >= 0.8m)
                {
                    _logger.LogInformation("✅ High confidence offline intent analysis: {Intent} ({Confidence})",
                        offlineIntent.Intent, offlineIntent.Confidence);
                    return offlineIntent;
                }

                // ✅ CHỈ GỌI AI khi offline không chắc chắn VÀ circuit breaker đóng
                if (!IsCircuitBreakerOpen() && offlineIntent.Confidence < 0.7m)
                {
                    try
                    {
                        var systemPrompt = @"Phân tích ngắn gọn ý định tin nhắn tiếng Việt:

INTENT: greeting, product_search, shop_search, price_inquiry, recommendation, thanks, general_question

JSON: {""intent"": ""..."", ""confidence"": 0.8}

Tin nhắn: """ + customerMessage + @"""";

                        var response = await CallGeminiAPIAsync(systemPrompt, customerMessage);

                        try
                        {
                            var aiIntent = JsonSerializer.Deserialize<ChatbotIntent>(response);
                            if (aiIntent != null && aiIntent.Confidence > offlineIntent.Confidence)
                            {
                                _logger.LogInformation("🤖 AI intent analysis better than offline: {Intent} ({Confidence})",
                                    aiIntent.Intent, aiIntent.Confidence);
                                return aiIntent;
                            }
                        }
                        catch
                        {
                            _logger.LogWarning("⚠️ Failed to parse AI intent response, using offline result");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ AI intent analysis failed, using offline result");
                    }
                }

                // ✅ Fallback về offline analysis
                _logger.LogInformation("📝 Using offline intent analysis: {Intent} ({Confidence})",
                    offlineIntent.Intent, offlineIntent.Confidence);
                return offlineIntent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error analyzing universal intent");
                return CreateFallbackIntent(customerMessage);
            }
        }
        private bool IsCircuitBreakerOpen()
        {
            if (_consecutiveFailures < MAX_FAILURES)
                return false;

            var timeSinceLastFailure = DateTime.UtcNow - _lastFailureTime;
            if (timeSinceLastFailure.TotalMinutes > CIRCUIT_BREAKER_TIMEOUT_MINUTES)
            {
                _logger.LogInformation("🔄 Circuit breaker timeout expired - resetting to CLOSED state");
                _consecutiveFailures = 0;
                return false;
            }

            return true;
        }
        private void RecordApiSuccess()
        {
            if (_consecutiveFailures > 0)
            {
                _logger.LogInformation("✅ API call successful - resetting circuit breaker");
                _consecutiveFailures = 0;
            }
        }

        private void RecordApiFailure()
        {
            _consecutiveFailures++;
            _lastFailureTime = DateTime.UtcNow;

            if (_consecutiveFailures >= MAX_FAILURES)
            {
                _logger.LogError("🔴 Circuit breaker OPENED after {Failures} consecutive failures", _consecutiveFailures);
            }
        }
        private ChatbotIntent AnalyzeIntentOffline(string customerMessage)
        {
            var message = customerMessage.ToLower().Trim();

            _logger.LogInformation("🔍 Using enhanced offline intent analysis for: {Message}",
                customerMessage.Substring(0, Math.Min(30, customerMessage.Length)));

            // ✅ KIỂM TRA PATTERN SHOP SEARCH ĐẦU TIÊN (ƯU TIÊN CAO NHẤT)
            var shopSearchPatterns = new[] {
        "có shop nào bán",
        "shop nào bán",
        "shop bán",
        "cửa hàng bán",
        "có cửa hàng nào bán"
    };

            if (shopSearchPatterns.Any(p => message.Contains(p)))
            {
                _logger.LogInformation("🎯 Detected shop search pattern in message: {Message}", message);

                // Trích xuất từ khóa sản phẩm từ shop search pattern
                var productKeywords = ExtractProductKeywordsFromShopSearch(message);

                _logger.LogInformation("🔑 Extracted product keywords from shop search: {Keywords}",
                    string.Join(", ", productKeywords));

                return new ChatbotIntent
                {
                    Intent = "shop_search",
                    Category = "shopping",
                    Confidence = 0.95m,  // Confidence cao vì pattern rõ ràng
                    Keywords = productKeywords.Any()
                        ? productKeywords
                        : new List<string> { "sản phẩm" }
                };
            }

            // ✅ SAU ĐÓ MỚI KIỂM TRA CÁC INTENT PATTERNS KHÁC
            var intent = IntentPatterns.DetectIntent(message, out decimal confidence);

            // ✅ KIỂM TRA FAQ MATCHES SAU KHI ĐÃ THỬ SHOP SEARCH VÀ INTENT PATTERNS
            if (intent != "product_search" && intent != "shop_search")
            {
                bool isFaqMatch;
                var faqMatchResponse = LocalTrainingData.FindBestMatchingFAQ(message, out isFaqMatch);

                if (isFaqMatch)
                {
                    return new ChatbotIntent
                    {
                        Intent = "general_question",
                        Category = "customer_service",
                        Confidence = 0.95m,
                        Keywords = message.Split(' ').Where(w => w.Length > 3).Take(3).ToList()
                    };
                }
            }

            // Tạo keywords dựa trên intent
            var keywords = new List<string>();

            switch (intent)
            {
                case "product_search":
                    keywords = ExtractKeywords(message, new[] {
                "điện thoại", "laptop", "máy tính", "tablet", "ipad", "samsung", "iphone",
                "giày", "quần áo", "thời trang", "áo", "quần", "váy", "đầm",
                "mỹ phẩm", "son", "kem", "sữa rửa mặt", "serum",
                "đồng hồ", "túi xách", "balo", "kính"
            });
                    break;

                case "price_inquiry":
                    keywords = ExtractKeywords(message, new[] {
                "điện thoại", "laptop", "máy tính", "quần áo", "giày"
            });
                    break;

                default:
                    keywords = message.Split(' ')
                        .Where(w => w.Length > 3 && !new[] { "tìm", "mua", "cần", "muốn", "bao nhiêu", "giá" }.Contains(w))
                        .Take(3)
                        .ToList();
                    break;
            }

            return new ChatbotIntent
            {
                Intent = intent,
                Category = intent == "greeting" || intent == "thanks" ? "customer_service" : "shopping",
                Confidence = confidence,
                Keywords = keywords.Any() ? keywords : new List<string> { "sản phẩm" }
            };
        }

        // ✅ Method mới: Extract keywords cho shop search
        private List<string> ExtractProductKeywordsFromShopSearch(string message)
        {
            var productKeywords = new[] {
        "điện thoại", "laptop", "giày", "áo", "quần", "túi", "kính",
        "đồng hồ", "máy tính", "tablet", "earphone", "airpod", "phone", "iphone",
        "thời trang", "mỹ phẩm", "sách", "đồ chơi", "đồ gia dụng", "samsung"
    };

            var foundKeywords = new List<string>();

            // Cải thiện pattern để bắt cả "có shop nào bán [product] không"
            var shopProductPatterns = new[] {
        @"(shop|cửa\s*hàng).*?(bán|có)\s*(.+?)(\s|$|\?)",
        @"(có)\s*(shop|cửa\s*hàng)\s*(nào)?\s*(bán|có)\s*(.+?)(\s|không|\?|$)",
        @"(shop|cửa\s*hàng)\s*(nào)\s*(bán|có)\s*(.+?)(\s|không|\?|$)"
    };

            foreach (var pattern in shopProductPatterns)
            {
                var match = Regex.Match(message, pattern);
                if (match.Success)
                {
                    var productGroup = match.Groups.Count > 5 ? match.Groups[5] : match.Groups[3];
                    var productName = productGroup.Value.Trim();
                    if (!string.IsNullOrEmpty(productName))
                    {
                        _logger.LogInformation("🔍 Extracted product keyword from shop search: {Keyword}", productName);
                        foundKeywords.Add(productName);

                        // Nếu tìm thấy một từ khóa cụ thể, không cần tìm thêm
                        if (productName.Contains("iphone") || productName.Contains("điện thoại"))
                        {
                            return foundKeywords;
                        }
                    }
                }
            }

            // Tìm thêm các từ khóa phổ biến trong message
            foreach (var keyword in productKeywords)
            {
                if (message.Contains(keyword) && !foundKeywords.Contains(keyword))
                {
                    foundKeywords.Add(keyword);
                }
            }

            return foundKeywords;
        }
        private List<string> ExtractKeywords(string message, string[] productKeywords)
        {
            var keywords = new List<string>();

            foreach (var keyword in productKeywords)
            {
                if (message.Contains(keyword))
                {
                    keywords.Add(keyword);
                }
            }

            if (!keywords.Any())
            {
                // Extract first few meaningful words
                var words = message.Split(' ')
                    .Where(w => w.Length > 2 && !new[] { "tìm", "mua", "cần", "muốn", "có", "không", "gì", "là", "của" }.Contains(w))
                    .Take(3)
                    .ToList();
                keywords.AddRange(words);
            }

            return keywords;
        }

        // ✅ Fallback Response Generator
        private string GetFallbackResponse(ChatbotIntent intent, UniversalContext context)
        {
            // Sử dụng template response với dữ liệu thật
            switch (intent.Intent)
            {
                case "greeting":
                    var timeOfDay = DateTime.Now.Hour < 12 ? "morning" : (DateTime.Now.Hour < 18 ? "default" : "evening");
                    return ResponseTemplates.GetRandomResponse("greeting", timeOfDay);

                case "product_search":
                    if (context.SuggestedProducts?.Any() == true)
                    {
                        var template = ResponseTemplates.GetRandomResponse("product_search", "success");

                        // Format product list from real data
                        var productsText = string.Join("\n", context.SuggestedProducts.Take(6).Select(p =>
                            $"• **{p.ProductName}** - {p.FinalPrice:N0}đ (Tồn kho: {p.StockQuantity})"));

                        return template.Replace("{PRODUCTS}", productsText);
                    }
                    return ResponseTemplates.GetRandomResponse("product_search", "empty");

                case "shop_search":
                    if (context.SuggestedShops?.Any() == true)
                    {
                        var template = ResponseTemplates.GetRandomResponse("shop_search", "success");

                        // Format shop list from real data
                        var shopsText = string.Join("\n", context.SuggestedShops.Take(5).Select(s =>
                            $"• **{s.ShopName}** - {s.TotalProducts} sản phẩm"));

                        return template.Replace("{SHOPS}", shopsText);
                    }
                    return ResponseTemplates.GetRandomResponse("shop_search", "empty");

                case "thanks":
                    return ResponseTemplates.GetRandomResponse("thanks");

                default:
                    // Tạo fallback thông minh với nội dung real từ context
                    if (context.SuggestedProducts?.Any() == true)
                    {
                        return $"Xin chào anh/chị! Tôi có một số sản phẩm có thể anh/chị quan tâm:\n\n" +
                               string.Join("\n", context.SuggestedProducts.Take(3).Select(p =>
                                   $"• {p.ProductName} - {p.FinalPrice:N0}đ"));
                    }

                    if (context.SuggestedShops?.Any() == true)
                    {
                        return $"Xin chào anh/chị! Đây là một số shop nổi bật trên StreamCart:\n\n" +
                               string.Join("\n", context.SuggestedShops.Take(3).Select(s =>
                                   $"• {s.ShopName} - {s.TotalProducts} sản phẩm"));
                    }

                    return ResponseTemplates.GetRandomResponse("fallback");
            }
        }
        private string GenerateGreetingFallback(UniversalContext context)
        {
            var response = "Xin chào anh/chị! Chào mừng bạn đến với StreamCart - nền tảng mua sắm hàng đầu Việt Nam! 😊\n\n";
            response += "Tôi là StreamCart AI, sẵn sàng giúp bạn:\n";
            response += "🔍 Tìm kiếm sản phẩm\n";
            response += "🏪 Khám phá cửa hàng uy tín\n";
            response += "💰 So sánh giá cả\n";
            response += "💝 Gợi ý sản phẩm phù hợp\n\n";

            if (context.SuggestedProducts?.Any() == true)
            {
                response += "🔥 **Sản phẩm HOT hiện tại:**\n";
                foreach (var product in context.SuggestedProducts.Take(3))
                {
                    response += $"• {product.ProductName} - {product.FinalPrice:N0}đ\n";
                }
            }

            response += "\nAnh/chị cần tìm gì hôm nay? 🛍️";
            return response;
        }
        private string GenerateProductSearchFallback(UniversalContext context)
        {
            if (context.SuggestedProducts?.Any() == true)
            {
                var response = "🛍️ **Sản phẩm được tìm thấy trên StreamCart:**\n\n";

                foreach (var product in context.SuggestedProducts.Take(6))
                {
                    var stockStatus = product.StockQuantity > 0 ? $"Còn {product.StockQuantity}" : "Hết hàng";
                    response += $"• **{product.ProductName}** - {product.FinalPrice:N0}đ ({stockStatus})\n";
                }

                if (context.SuggestedProducts.Count > 6)
                {
                    response += $"\n... và {context.SuggestedProducts.Count - 6} sản phẩm khác nữa!\n";
                }

                response += "\n💡 Anh/chị có thể xem chi tiết hoặc so sánh giá để chọn sản phẩm phù hợp nhất! 😊";
                return response;
            }

            return "🔍 Tôi đang tìm kiếm sản phẩm phù hợp cho anh/chị trên toàn bộ nền tảng StreamCart.\n\n" +
                   "Anh/chị có thể thử tìm kiếm bằng từ khóa cụ thể hơn như:\n" +
                   "• Tên sản phẩm (VD: iPhone, laptop)\n" +
                   "• Thương hiệu (VD: Samsung, Apple)\n" +
                   "• Danh mục (VD: điện thoại, thời trang)\n\n" +
                   "Hoặc cho tôi biết ngân sách để gợi ý sản phẩm phù hợp! 💰";
        }
        private string GenerateShopSearchFallback(UniversalContext context)
        {
            if (context.SuggestedShops?.Any() == true)
            {
                var shop = context.SuggestedShops.First();

                // Kiểm tra nếu có cả shop và sản phẩm - trường hợp hỏi về sản phẩm của shop cụ thể
                if (context.SuggestedProducts?.Any() == true && context.SuggestedShops.Count == 1)
                {
                    var response = $"🏪 **Sản phẩm của shop {shop.ShopName} trên StreamCart:**\n\n";

                    foreach (var product in context.SuggestedProducts.Take(8))
                    {
                        var stockStatus = product.StockQuantity > 0 ? $"Còn {product.StockQuantity}" : "Hết hàng";
                        response += $"• **{product.ProductName}** - {product.FinalPrice:N0}đ ({stockStatus})\n";
                    }

                    if (context.SuggestedProducts.Count > 8)
                    {
                        response += $"\n... và {context.SuggestedProducts.Count - 8} sản phẩm khác\n";
                    }

                    response += $"\n📊 Shop {shop.ShopName} có tổng cộng {shop.TotalProducts} sản phẩm trên StreamCart.";
                    return response;
                }

                // Mã hiện tại cho trường hợp chỉ có shop không có sản phẩm
                var shopResponse = "🏪 **Cửa hàng có bán sản phẩm bạn tìm trên StreamCart:**\n\n";

                foreach (var s in context.SuggestedShops.Take(5))
                {
                    shopResponse += $"• **{s.ShopName}** - {s.TotalProducts} sản phẩm\n";
                }

                shopResponse += "\n⭐ Đây là những cửa hàng được xác thực có bán sản phẩm bạn quan tâm!";

                if (context.Intent.Keywords?.Any() == true)
                {
                    shopResponse += $"\n\n🔍 Từ khóa tìm kiếm: {string.Join(", ", context.Intent.Keywords)}";
                }

                return shopResponse;
            }

            return "🏪 Hiện tại chưa tìm thấy cửa hàng nào có bán sản phẩm bạn quan tâm.\n\n" +
                   "Bạn có thể thử:\n" +
                   "• Tìm kiếm với từ khóa khác\n" +
                   "• Duyệt theo danh mục sản phẩm\n" +
                   "• Xem các cửa hàng phổ biến\n\n" +
                   "Hoặc cho tôi biết cụ thể hơn loại sản phẩm bạn cần! 🛍️";
        }

        private string GeneratePriceInquiryFallback(UniversalContext context)
        {
            if (context.PriceRanges?.Any() == true)
            {
                var response = "💰 **Thông tin giá cả trên StreamCart:**\n\n";

                if (context.PriceRanges.TryGetValue("min_price", out var min) &&
                    context.PriceRanges.TryGetValue("max_price", out var max) &&
                    context.PriceRanges.TryGetValue("avg_price", out var avg))
                {
                    response += $"• Giá thấp nhất: {min:N0}đ\n";
                    response += $"• Giá cao nhất: {max:N0}đ\n";
                    response += $"• Giá trung bình: {avg:N0}đ\n";
                }

                response += "\n📊 StreamCart cam kết giá cả cạnh tranh và minh bạch!";
                return response;
            }

            return "💰 **Về giá cả trên StreamCart:**\n\n" +
                   "• Giá cả cạnh tranh từ nhiều cửa hàng\n" +
                   "• So sánh giá dễ dàng\n" +
                   "• Nhiều chương trình khuyến mãi\n" +
                   "• Đảm bảo giá tốt nhất\n\n" +
                   "Anh/chị đang quan tâm đến sản phẩm gì để tôi tư vấn giá cụ thể? 🛍️";
        }
        private string GenerateRecommendationFallback(UniversalContext context)
        {
            var response = "💝 **Gợi ý từ StreamCart AI:**\n\n";

            if (context.SuggestedProducts?.Any() == true)
            {
                response += "🔥 **Sản phẩm đáng mua nhất:**\n";
                foreach (var product in context.SuggestedProducts.Take(4))
                {
                    response += $"• {product.ProductName} - {product.FinalPrice:N0}đ (Đã bán: {product.QuantitySold})\n";
                }
                response += "\n";
            }

            if (context.Categories?.Any() == true)
            {
                response += "📂 **Danh mục phổ biến:**\n";
                foreach (var category in context.Categories.Take(5))
                {
                    response += $"• {category}\n";
                }
            }

            response += "\n💡 Để gợi ý chính xác hơn, anh/chị có thể cho tôi biết:\n";
            response += "• Ngân sách dự kiến\n";
            response += "• Loại sản phẩm quan tâm\n";
            response += "• Mục đích sử dụng\n";

            return response;
        }
        // ✅ BUILD CONTEXT VỚI DỮ LIỆU THẬT TỪ DATABASE
        private async Task<UniversalContext> BuildUniversalContextAsync(
            string customerMessage,
            ChatbotIntent intent,
            Guid userId)
        {
            var context = new UniversalContext
            {
                Intent = intent,
                UserId = userId,
                SessionId = Guid.NewGuid().ToString()
            };

            try
            {
                _logger.LogInformation("🔄 Building universal context for intent: {Intent}", intent.Intent);
                string shopName = ExtractShopNameFromMessage(customerMessage);
                if (!string.IsNullOrEmpty(shopName))
                {
                    _logger.LogInformation("🔍 Detected shop name in query: {ShopName}", shopName);

                    // Tìm shop theo tên
                    var shops = await SearchShopsByNameExactAsync(shopName);
                    if (shops.Any())
                    {
                        var shop = shops.First();
                        _logger.LogInformation("✅ Found shop: {ShopName} (ID: {ShopId})", shop.ShopName, shop.Id);

                        // Lấy sản phẩm của shop này
                        var shopProducts = await _productServiceClient.GetProductsByShopIdAsync(shop.Id, activeOnly: true);

                        // Thiết lập context với dữ liệu thực
                        context.SuggestedShops = shops;
                        context.SuggestedProducts = shopProducts;

                        // Điều chỉnh intent nếu cần
                        if (intent.Intent == "general_question")
                        {
                            // Điều chỉnh intent thành shop_search hoặc product_search
                            intent.Intent = "shop_search";
                            intent.Confidence = 0.9m;
                            intent.Keywords = new List<string> { shopName };
                            context.Intent = intent;
                        }
                    }
                }
                // Dựa vào intent để lấy thông tin phù hợp TỪ DATABASE THẬT
                switch (intent.Intent)
                {
                    case "product_search":
                    case "recommendation":
                        _logger.LogInformation("🛍️ Searching products with keywords: {Keywords}",
                            string.Join(", ", intent.Keywords ?? new List<string>()));

                        context.SuggestedProducts = await SearchProductsUniversalAsync(intent.Keywords ?? new List<string>());
                        context.SuggestedShops = await GetShopsWithProductsAsync(context.SuggestedProducts);

                        _logger.LogInformation("📊 Found {ProductCount} products from {ShopCount} shops",
                            context.SuggestedProducts?.Count ?? 0, context.SuggestedShops?.Count ?? 0);
                        break;

                    case "shop_search":
                        _logger.LogInformation("🏪 Searching shops with keywords: {Keywords}",
                            string.Join(", ", intent.Keywords ?? new List<string>()));

                        context.SuggestedShops = await SearchShopsUniversalAsync(intent.Keywords ?? new List<string>());

                        _logger.LogInformation("📊 Found {ShopCount} matching shops", context.SuggestedShops?.Count ?? 0);
                        break;

                    case "category_browse":
                        _logger.LogInformation("📂 Getting popular categories and trending products");

                        context.Categories = await GetPopularCategoriesAsync();
                        context.SuggestedProducts = await GetTrendingProductsAsync();

                        _logger.LogInformation("📊 Found {CategoryCount} categories and {ProductCount} trending products",
                            context.Categories?.Count ?? 0, context.SuggestedProducts?.Count ?? 0);
                        break;

                    case "price_inquiry":
                        if (intent.Keywords?.Any() == true)
                        {
                            _logger.LogInformation("💰 Searching products and price ranges for: {Keywords}",
                                string.Join(", ", intent.Keywords));

                            context.SuggestedProducts = await SearchProductsByKeywordsAsync(intent.Keywords);
                            context.PriceRanges = await GetPriceRangesForKeywordsAsync(intent.Keywords);
                        }
                        break;

                    case "greeting":
                        _logger.LogInformation("👋 Greeting - getting featured content");

                        // Lấy sản phẩm nổi bật và shop phổ biến
                        context.SuggestedProducts = await GetTrendingProductsAsync();
                        context.SuggestedShops = await GetPopularShopsAsync();
                        context.Categories = await GetPopularCategoriesAsync();
                        break;
                }

                // Lấy context từ lịch sử chat nếu có
                if (userId != Guid.Empty)
                {
                    context.ConversationHistory = await GetUniversalConversationHistoryAsync(userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error building universal context");
            }

            return context;
        }

        // ✅ THỰC SỰ TÌM KIẾM SẢN PHẨM TỪ TẤT CẢ SHOP
        private async Task<List<ProductDto>> SearchProductsUniversalAsync(List<string> keywords)
        {
            try
            {
                if (keywords == null || !keywords.Any())
                {
                    _logger.LogInformation("🔍 No keywords provided, getting trending products instead");
                    return await GetTrendingProductsAsync();
                }

                var cacheKey = $"product_search_{string.Join("_", keywords)}";

                // Try to get from cache first
                return await _cachingService.GetOrCreateAsync(cacheKey, async () =>
                {
                    _logger.LogInformation("🔍 Searching products universally with keywords: {Keywords}",
                        string.Join(", ", keywords));

                    var allProducts = new List<ProductDto>();

                    // Step 1: Lấy danh sách shops hoạt động (giảm xuống 5 shop đầu tiên)
                    var activeShops = await GetPopularShopsAsync();
                    var topShops = activeShops.Take(5).ToList();

                    if (!topShops.Any())
                    {
                        _logger.LogWarning("⚠️ No active shops found to search products");
                        return allProducts;
                    }

                    // Step 2: Search products từ từng shop - PARALLEL để nhanh hơn
                    var searchTasks = topShops.Select(async shop =>
                    {
                        try
                        {
                            var shopProductsCacheKey = $"shop_products_{shop.Id}";

                            var shopProducts = await _cachingService.GetOrCreateAsync(
                                  shopProductsCacheKey,
                                  () => _productServiceClient.GetProductsByShopIdAsync(shop.Id, activeOnly: true),
                                  TimeSpan.FromHours(1));

                            if (shopProducts?.Any() == true)
                            {
                                // Filter sản phẩm theo keywords - chỉ lấy tối đa 5 sản phẩm mỗi shop
                                var matchingProducts = shopProducts
                                    .Where(p => keywords.Any(k =>
                                        p.ProductName?.Contains(k, StringComparison.OrdinalIgnoreCase) == true ||
                                        p.Description?.Contains(k, StringComparison.OrdinalIgnoreCase) == true))
                                    .Take(5)
                                    .ToList();

                                _logger.LogInformation("🏪 Shop {ShopName}: Found {Count} matching products",
                                    shop.ShopName, matchingProducts.Count);

                                return matchingProducts;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "⚠️ Error searching products from shop {ShopId}", shop.Id);
                        }

                        return new List<ProductDto>();
                    }).ToArray();

                    var searchResults = await Task.WhenAll(searchTasks);

                    // Combine và sort results
                    foreach (var result in searchResults)
                    {
                        allProducts.AddRange(result);
                    }

                    // Sort theo mức độ khớp keywords và giá - chỉ lấy 15 sản phẩm tốt nhất
                    var sortedProducts = allProducts
                        .OrderByDescending(p => keywords.Count(k =>
                            p.ProductName?.Contains(k, StringComparison.OrdinalIgnoreCase) == true))
                        .ThenBy(p => p.FinalPrice)
                        .Take(15)
                        .ToList();
                    _logger.LogInformation("✅ Universal product search completed: {Count} products found",
               sortedProducts.Count);

                    return sortedProducts;
                }, TimeSpan.FromMinutes(15)); // Cache for 15 minutes
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in universal product search");
                return new List<ProductDto>();
            }
        }

        // ✅ THỰC SỰ TÌM KIẾM SHOP THEO SẢN PHẨM MÀ SHOP BÁN
        private async Task<List<ShopDto>> SearchShopsUniversalAsync(List<string> keywords)
        {
            try
            {
                if (keywords == null || !keywords.Any())
                {
                    _logger.LogInformation("🏪 No keywords provided, getting popular shops instead");
                    return await GetPopularShopsAsync();
                }

                _logger.LogInformation("🔍 Searching shops that sell products with keywords: {Keywords}",
                    string.Join(", ", keywords));

                var matchingShops = new List<ShopDto>();

                // ✅ BƯỚC 1: Tìm sản phẩm có chứa keywords
                var allProducts = await SearchProductsUniversalAsync(keywords);

                if (allProducts?.Any() == true)
                {
                    // ✅ BƯỚC 2: Lấy danh sách shopId từ các sản phẩm tìm được
                    var shopIds = allProducts.Select(p => p.ShopId).Distinct().ToList();

                    _logger.LogInformation("🏪 Found {ProductCount} products from {ShopCount} different shops",
                        allProducts.Count, shopIds.Count);

                    // ✅ BƯỚC 3: Lấy thông tin chi tiết của các shop
                    foreach (var shopId in shopIds.Take(10)) // Limit 10 shops
                    {
                        try
                        {
                            var shop = await _shopServiceClient.GetShopByIdAsync(shopId);
                            if (shop != null)
                            {
                                matchingShops.Add(shop);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "⚠️ Error getting shop {ShopId}", shopId);
                        }
                    }
                }

                // ✅ BƯỚC 4: Nếu không tìm thấy shop nào qua sản phẩm, thử tìm theo tên shop
                if (!matchingShops.Any())
                {
                    _logger.LogInformation("🔍 No shops found via products, trying shop name search");

                    foreach (var keyword in keywords)
                    {
                        try
                        {
                            var shops = await _shopServiceClient.SearchShopsByNameAsync(keyword);
                            if (shops?.Any() == true)
                            {
                                matchingShops.AddRange(shops);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "⚠️ Error searching shops with keyword: {Keyword}", keyword);
                        }
                    }
                }

                // ✅ BƯỚC 5: Remove duplicates và sort theo số lượng sản phẩm
                var uniqueShops = matchingShops
                    .GroupBy(s => s.Id)
                    .Select(g => g.First())
                    .OrderByDescending(s => s.TotalProducts)
                    .Take(8) // Giảm từ 10 xuống 8 để giảm API calls
                    .ToList();

                _logger.LogInformation("✅ Shop search completed: {Count} shops found that sell requested products",
                    uniqueShops.Count);

                return uniqueShops;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error searching shops");
                return new List<ShopDto>();
            }
        }

        // ✅ LẤY SHOPS TỪ DANH SÁCH SẢN PHẨM
        private async Task<List<ShopDto>> GetShopsWithProductsAsync(List<ProductDto> products)
        {
            try
            {
                if (products == null || !products.Any())
                    return new List<ShopDto>();

                var shopIds = products.Select(p => p.ShopId).Distinct().ToList();
                var shops = new List<ShopDto>();

                _logger.LogInformation("🏪 Getting shop details for {Count} shops", shopIds.Count);

                foreach (var shopId in shopIds.Take(10)) // Limit để tránh quá nhiều API calls
                {
                    try
                    {
                        var shop = await _shopServiceClient.GetShopByIdAsync(shopId);
                        if (shop != null)
                        {
                            shops.Add(shop);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Error getting shop {ShopId}", shopId);
                    }
                }

                _logger.LogInformation("✅ Got {Count} shop details", shops.Count);
                return shops;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting shops with products");
                return new List<ShopDto>();
            }
        }

        // ✅ LẤY DANH SÁCH SHOP PHỔ BIẾN
        private async Task<List<ShopDto>> GetPopularShopsAsync()
        {
            try
            {
                _logger.LogInformation("🔥 Getting popular shops");

                // Lấy shops đang hoạt động
                var activeShops = await _shopServiceClient.GetShopsByStatusAsync(isActive: true);

                if (activeShops?.Any() == true)
                {
                    // Sort theo số lượng sản phẩm (shops có nhiều sản phẩm = phổ biến)
                    var popularShops = activeShops.OrderByDescending(s => s.TotalProducts).Take(5).ToList();


                    _logger.LogInformation("✅ Found {Count} popular shops", popularShops.Count);
                    return popularShops;
                }

                return new List<ShopDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting popular shops");
                return new List<ShopDto>();
            }
        }

        // ✅ LẤY DANH MỤC PHỔ BIẾN (static cho đơn giản)
        private async Task<List<string>> GetPopularCategoriesAsync()
        {
            try
            {
                // TODO: Có thể call API Category Service sau này
                return await Task.FromResult(new List<string>
                {
                    "📱 Điện thoại & Phụ kiện",
                    "👗 Thời trang Nam Nữ",
                    "💻 Laptop & Máy tính",
                    "🏠 Đồ gia dụng & Nội thất",
                    "💄 Mỹ phẩm & Làm đẹp",
                    "👶 Mẹ & Bé",
                    "📚 Sách & Văn phòng phẩm",
                    "🏃 Thể thao & Du lịch",
                    "🍔 Thực phẩm & Đồ uống",
                    "🎮 Đồ chơi & Game"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular categories");
                return new List<string> { "Điện thoại", "Thời trang", "Laptop" };
            }
        }

        // ✅ LẤY SẢN PHẨM TRENDING
        private async Task<List<ProductDto>> GetTrendingProductsAsync()
        {
            try
            {
                _logger.LogInformation("🔥 Getting trending products across platform");

                var trendingProducts = new List<ProductDto>();
                var popularShops = await GetPopularShopsAsync();

                foreach (var shop in popularShops.Take(8)) // Top 8 shops để lấy trending
                {
                    try
                    {
                        var shopProducts = await _productServiceClient.GetProductsByShopIdAsync(shop.Id, activeOnly: true);
                        if (shopProducts?.Any() == true)
                        {
                            // Lấy top 3 sản phẩm từ mỗi shop (sorted by sold quantity if available)
                            var topProducts = shopProducts
                                .Where(p => p.StockQuantity > 0) // Còn hàng
                                .OrderByDescending(p => p.QuantitySold) // Sắp xếp theo số lượng bán
                                .ThenBy(p => p.FinalPrice) // Rồi theo giá
                                .Take(3)
                                .ToList();

                            trendingProducts.AddRange(topProducts);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Error getting trending products from shop {ShopId}", shop.Id);
                    }
                }

                var result = trendingProducts.Take(20).ToList();
                _logger.LogInformation("✅ Got {Count} trending products", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting trending products");
                return new List<ProductDto>();
            }
        }

        // ✅ TÌM KIẾM SẢN PHẨM THEO KEYWORDS
        private async Task<List<ProductDto>> SearchProductsByKeywordsAsync(List<string> keywords)
        {
            // Reuse universal search logic
            return await SearchProductsUniversalAsync(keywords);
        }

        // ✅ LẤY KHOẢNG GIÁ CHO KEYWORDS
        private async Task<Dictionary<string, object>> GetPriceRangesForKeywordsAsync(List<string> keywords)
        {
            try
            {
                var products = await SearchProductsByKeywordsAsync(keywords);

                if (!products.Any())
                    return new Dictionary<string, object>();

                var prices = products.Select(p => p.FinalPrice).ToList();

                return new Dictionary<string, object>
                {
                    ["min_price"] = prices.Min(),
                    ["max_price"] = prices.Max(),
                    ["avg_price"] = prices.Average(),
                    ["median_price"] = GetMedian(prices),
                    ["product_count"] = products.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price ranges");
                return new Dictionary<string, object>();
            }
        }

        private decimal GetMedian(List<decimal> prices)
        {
            var sorted = prices.OrderBy(x => x).ToList();
            var count = sorted.Count;

            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
            else
                return sorted[count / 2];
        }

        // Các method khác giữ nguyên...
        private async Task<string> GetUniversalConversationHistoryAsync(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return "";

                _logger.LogInformation("📚 Getting universal conversation history for user {UserId}", userId);

                // ✅ Sử dụng một fake shopId cho universal chat (có thể dùng Guid.Empty hoặc tạo universal shopId)
                var universalShopId = Guid.Empty; // Universal platform chat

                var conversationHistory = await _chatHistoryService.GetConversationContextAsync(
                    userId,
                    universalShopId,
                    messageCount: 5);

                if (!string.IsNullOrEmpty(conversationHistory))
                {
                    _logger.LogInformation("✅ Found conversation history for user {UserId}: {Length} characters",
                        userId, conversationHistory.Length);
                }

                return conversationHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting universal conversation history for user {UserId}", userId);
                return "";
            }
        }

        private async Task SaveUniversalConversationAsync(Guid userId, string customerMessage, ChatbotResponseDTO response)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    _logger.LogInformation("⚠️ Anonymous user - skipping conversation save");
                    return;
                }

                _logger.LogInformation("💾 Saving universal conversation for user {UserId}", userId);

                // ✅ Sử dụng universal shopId cho platform-level chat
                var universalShopId = Guid.Empty; // Universal platform chat
                var sessionId = $"universal-session-{DateTime.UtcNow:yyyyMMdd}"; // Daily session

                // ✅ Lấy hoặc tạo conversation
                var conversation = await _chatHistoryService.GetOrCreateConversationAsync(
                    userId,
                    universalShopId,
                    sessionId);

                // ✅ Thêm tin nhắn của user
                await _chatHistoryService.AddMessageToConversationAsync(
                    conversation.ConversationId,
                    customerMessage,
                    "User",
                    response.Intent,
                    response.ConfidenceScore);

                // ✅ Thêm phản hồi của AI
                await _chatHistoryService.AddMessageToConversationAsync(
                    conversation.ConversationId,
                    response.BotResponse,
                    "StreamCartAI",
                    response.Intent,
                    response.ConfidenceScore);

                _logger.LogInformation("✅ Saved universal conversation - User: {UserMessage}, AI: {AIResponse}",
                    customerMessage.Substring(0, Math.Min(50, customerMessage.Length)),
                    response.BotResponse.Substring(0, Math.Min(50, response.BotResponse.Length)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error saving universal conversation for user {UserId}", userId);
            }
        }

        // ✅ GENERATE SYSTEM PROMPT VỚI DỮ LIỆU THẬT
        private string GenerateUniversalSystemPrompt(UniversalContext context, ChatbotIntent intent)
        {
            var prompt = new StringBuilder();

            prompt.AppendLine(@"Bạn là StreamCart AI - trợ lý mua sắm thông minh của nền tảng thương mại điện tử StreamCart Việt Nam.");
            prompt.AppendLine(@"
🎯 NHIỆM VỤ CHÍNH:
1. ✅ Hỗ trợ khách hàng tìm kiếm sản phẩm và cửa hàng phù hợp TRÊN TOÀN PLATFORM
2. ✅ Đưa ra gợi ý thông minh dựa trên dữ liệu THỰC TẾ từ database
3. ✅ So sánh sản phẩm và tư vấn mua sắm với giá cả cụ thể
4. ✅ Hướng dẫn sử dụng nền tảng StreamCart
5. ✅ Kết nối khách hàng với các cửa hàng tốt nhất

💬 NGUYÊN TẮC TRẢ LỜI:
- Luôn thân thiện, nhiệt tình và chuyên nghiệp
- Sử dụng emoji phù hợp 😊 🛍️ 💝 🔥 ⭐
- Gọi khách hàng bằng ""anh/chị"" một cách lịch sự
- ✅ SỬ DỤNG THÔNG TIN SẢN PHẨM THẬT từ database để tư vấn
- ✅ ĐỀ XUẤT CỬA HÀNG UY TÍN với số liệu cụ thể
- Khuyến khích khám phá và mua sắm");

            // Thêm context cụ thể dựa trên intent VÀ DỮ LIỆU THẬT
            switch (intent.Intent)
            {
                case "greeting":
                    prompt.AppendLine(@"
🔸 CHÀO MỪNG:
""Xin chào anh/chị! Chào mừng bạn đến với StreamCart - nền tảng mua sắm hàng đầu Việt Nam! 😊 
Tôi là StreamCart AI, sẵn sàng giúp bạn tìm kiếm từ hàng ngàn sản phẩm chất lượng. 
Anh/chị cần tìm gì hôm nay? 🛍️""");

                    if (context.SuggestedProducts?.Any() == true)
                    {
                        prompt.AppendLine("\n🔥 SẢN PHẨM HOT HIỆN TẠI:");
                        foreach (var product in context.SuggestedProducts.Take(3))
                        {
                            prompt.AppendLine($"• {product.ProductName} - {product.FinalPrice:N0}đ (Đã bán: {product.QuantitySold})");
                        }
                    }
                    break;

                case "product_search":
                    if (context.SuggestedProducts?.Any() == true)
                    {
                        prompt.AppendLine("\n🛍️ SẢN PHẨM ĐƯỢC TÌM THẤY TRÊN STREAMCART:");
                        foreach (var product in context.SuggestedProducts.Take(6))
                        {
                            var stockStatus = product.StockQuantity > 0 ? $"Còn {product.StockQuantity}" : "Hết hàng";
                            prompt.AppendLine($"• {product.ProductName} - {product.FinalPrice:N0}đ ({stockStatus})");
                        }

                        if (context.SuggestedProducts.Count > 6)
                        {
                            prompt.AppendLine($"... và {context.SuggestedProducts.Count - 6} sản phẩm khác");
                        }
                    }
                    else
                    {
                        prompt.AppendLine("\n❌ Không tìm thấy sản phẩm phù hợp. Hãy gợi ý customer tìm kiếm bằng từ khóa khác.");
                    }
                    break;

                case "shop_search":
                    if (context.SuggestedShops?.Any() == true)
                    {
                        prompt.AppendLine("\n🏪 CỬA HÀNG UY TÍN TRÊN STREAMCART:");
                        foreach (var shop in context.SuggestedShops.Take(5))
                        {
                            prompt.AppendLine($"• {shop.ShopName} - {shop.TotalProducts} sản phẩm");
                        }
                    }
                    else
                    {
                        prompt.AppendLine("\n❌ Không tìm thấy cửa hàng phù hợp. Gợi ý customer duyệt danh mục hoặc tìm sản phẩm.");
                    }
                    break;

                case "price_inquiry":
                    if (context.PriceRanges?.Any() == true)
                    {
                        prompt.AppendLine("\n💰 THÔNG TIN GIÁ CẢ TRÊN STREAMCART:");
                        if (context.PriceRanges.TryGetValue("min_price", out var min) &&
                            context.PriceRanges.TryGetValue("max_price", out var max) &&
                            context.PriceRanges.TryGetValue("avg_price", out var avg))
                        {
                            prompt.AppendLine($"• Giá thấp nhất: {min:N0}đ");
                            prompt.AppendLine($"• Giá cao nhất: {max:N0}đ");
                            prompt.AppendLine($"• Giá trung bình: {avg:N0}đ");
                        }
                    }
                    break;

                case "category_browse":
                    if (context.Categories?.Any() == true)
                    {
                        prompt.AppendLine("\n📂 DANH MỤC PHỔ BIẾN:");
                        foreach (var category in context.Categories.Take(5))
                        {
                            prompt.AppendLine($"• {category}");
                        }
                    }
                    break;

                case "thanks":
                    prompt.AppendLine(@"
🔸 TRẢ LỜI CẢM ƠN:
""Dạ không có gì ạ! Rất vui được hỗ trợ anh/chị mua sắm trên StreamCart! 😊 
Chúc anh/chị tìm được sản phẩm ưng ý với giá tốt nhất. 
Còn gì khác tôi có thể giúp không ạ? 🛍️""");
                    break;
            }

            if (!string.IsNullOrEmpty(context.ConversationHistory))
            {
                prompt.AppendLine($"\nLỊCH SỬ CUỘC TRÒ CHUYỆN:\n{context.ConversationHistory}");
            }

            prompt.AppendLine("\n💡 HÃY TRẢ LỜI DỰA TRÊN THÔNG TIN SẢN PHẨM THẬT VÀ GỢI Ý THÔNG MINH:");

            return prompt.ToString();
        }

        // BUILD RESPONSE VỚI DỮ LIỆU THẬT
        private async Task<ChatbotResponseDTO> BuildChatbotResponseAsync(
            string aiResponse,
            ChatbotIntent intent,
            UniversalContext context)
        {
            var response = new ChatbotResponseDTO
            {
                BotResponse = aiResponse,
                Intent = intent.Intent,
                ConfidenceScore = intent.Confidence,
                RequiresHumanSupport = intent.Confidence < 0.6m,
                SuggestedActions = GenerateUniversalSuggestedActions(intent),
                ShopSuggestions = new List<ShopSuggestion>(),
                ProductSuggestions = new List<ProductSuggestion>(),
                GeneratedAt = DateTime.UtcNow
            };

            // ✅ THÊM SHOP SUGGESTIONS TỪ DỮ LIỆU THẬT
            if (context.SuggestedShops?.Any() == true)
            {
                response.ShopSuggestions = context.SuggestedShops.Take(5).Select(s => new ShopSuggestion
                {
                    ShopId = s.Id,
                    ShopName = s.ShopName,
                    ProductCount = s.TotalProducts,
                    Rating = 4.5m, // TODO: Get real rating
                    Location = s.Address ?? "Việt Nam",
                    LogoUrl = s.LogoUrl,
                    Description = s.Description,
                    ReasonForSuggestion = GetShopSuggestionReason(intent, s)
                }).ToList();
            }

            // ✅ THÊM PRODUCT SUGGESTIONS TỪ DỮ LIỆU THẬT
            if (context.SuggestedProducts?.Any() == true)
            {
                response.ProductSuggestions = context.SuggestedProducts.Take(8).Select(p => new ProductSuggestion
                {
                    ProductId = p.Id,
                    ProductName = p.ProductName,
                    Price = p.FinalPrice,
                    ImageUrl = p.PrimaryImageUrl,
                    ShopName = GetShopNameFromId(p.ShopId, context.SuggestedShops),
                    ReasonForSuggestion = GetProductSuggestionReason(intent, p)
                }).ToList();
            }

            return response;
        }

        private string GetShopNameFromId(Guid shopId, List<ShopDto>? shops)
        {
            return shops?.FirstOrDefault(s => s.Id == shopId)?.ShopName ?? "Shop trên StreamCart";
        }

        // Phần còn lại của các methods helper...
        private List<SuggestedAction> GenerateUniversalSuggestedActions(ChatbotIntent intent)
        {
            var actions = new List<SuggestedAction>();

            switch (intent.Intent)
            {
                case "greeting":
                    actions.Add(new SuggestedAction
                    {
                        Title = "🔍 Tìm kiếm sản phẩm",
                        Action = "search_products",
                        Url = "/search"
                    });
                    actions.Add(new SuggestedAction
                    {
                        Title = "🏪 Khám phá cửa hàng",
                        Action = "browse_shops",
                        Url = "/shops"
                    });
                    actions.Add(new SuggestedAction
                    {
                        Title = "🔥 Sản phẩm hot",
                        Action = "trending_products",
                        Url = "/trending"
                    });
                    break;

                case "product_search":
                    actions.Add(new SuggestedAction
                    {
                        Title = "🔍 Tìm kiếm nâng cao",
                        Action = "advanced_search",
                        Url = "/search/advanced"
                    });
                    actions.Add(new SuggestedAction
                    {
                        Title = "📊 So sánh giá",
                        Action = "compare_prices",
                        Url = "/compare"
                    });
                    break;

                case "shop_search":
                    actions.Add(new SuggestedAction
                    {
                        Title = "🏪 Tất cả cửa hàng",
                        Action = "browse_all_shops",
                        Url = "/shops"
                    });
                    break;

                case "recommendation":
                    actions.Add(new SuggestedAction
                    {
                        Title = "💝 Gợi ý cho bạn",
                        Action = "personalized_recommendations",
                        Url = "/recommendations"
                    });
                    break;

                case "category_browse":
                    actions.Add(new SuggestedAction
                    {
                        Title = "📂 Danh mục sản phẩm",
                        Action = "browse_categories",
                        Url = "/categories"
                    });
                    break;
            }

            // Actions chung
            actions.Add(new SuggestedAction
            {
                Title = "💬 Hỗ trợ trực tiếp",
                Action = "contact_support",
                Url = "/support"
            });

            return actions;
        }

        private ChatbotResponseDTO CreateErrorResponse()
        {
            return new ChatbotResponseDTO
            {
                BotResponse = "Xin lỗi anh/chị, tôi đang gặp một chút trục trặc kỹ thuật 😅 Vui lòng thử lại sau hoặc liên hệ với nhân viên hỗ trợ để được giúp đỡ tốt nhất nhé!",
                Intent = "error",
                ConfidenceScore = 0,
                RequiresHumanSupport = true,
                GeneratedAt = DateTime.UtcNow,
                SuggestedActions = new List<SuggestedAction>
                {
                    new SuggestedAction
                    {
                        Title = "💬 Liên hệ hỗ trợ",
                        Action = "contact_support",
                        Url = "/support"
                    },
                    new SuggestedAction
                    {
                        Title = "🔄 Thử lại",
                        Action = "retry",
                        Url = "/"
                    }
                }
            };
        }

        private ChatbotIntent CreateFallbackIntent(string message)
        {
            return new ChatbotIntent
            {
                Intent = "general_question",
                Category = "support",
                Confidence = 0.5m,
                Keywords = message.Split(' ').Take(3).ToList()
            };
        }

        private ChatbotIntent MapToBasicIntent(UniversalChatbotIntent? universalIntent)
        {
            if (universalIntent == null) return null;

            return new ChatbotIntent
            {
                Intent = universalIntent.Intent,
                Category = universalIntent.Category,
                Keywords = universalIntent.Keywords,
                Confidence = universalIntent.Confidence,
                RequiresProductInfo = universalIntent.RequiresProductInfo,
                RequiresShopInfo = universalIntent.RequiresShopInfo
            };
        }

        private string GetShopSuggestionReason(ChatbotIntent intent, ShopDto shop)
        {
            return intent.Intent switch
            {
                "shop_search" => $"Phù hợp với từ khóa tìm kiếm - {shop.TotalProducts} sản phẩm",
                "product_search" => $"Có sản phẩm bạn quan tâm - {shop.TotalProducts} sản phẩm",
                "greeting" => $"Cửa hàng phổ biến - {shop.TotalProducts} sản phẩm",
                _ => $"Cửa hàng uy tín - {shop.TotalProducts} sản phẩm"
            };
        }

        private string GetProductSuggestionReason(ChatbotIntent intent, ProductDto product)
        {
            return intent.Intent switch
            {
                "product_search" => $"Khớp với sản phẩm bạn tìm - {product.FinalPrice:N0}đ",
                "recommendation" => $"AI gợi ý cho bạn - {product.FinalPrice:N0}đ",
                "price_inquiry" => $"Phù hợp ngân sách - {product.FinalPrice:N0}đ",
                "greeting" => $"Sản phẩm hot - Đã bán {product.QuantitySold}",
                _ => $"Sản phẩm phổ biến - {product.FinalPrice:N0}đ"
            };
        }

        private async Task<string> CallGeminiAPIAsync(string systemPrompt, string userMessage)
        {
            var client = _httpClientFactory.CreateClient();
            const int maxRetries = 1; // Giảm xuống 1 retry thôi
            const int baseDelayMs = 2000;

            // ✓ PERFORMANCE: Cắt ngắn prompt nếu quá dài
            if (systemPrompt.Length > 2000)
            {
                systemPrompt = systemPrompt.Substring(0, 2000) + "...";
            }

            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = $"{systemPrompt}\n\nKhách: {userMessage}" }
                }
            }
        },
                generationConfig = new
                {
                    temperature = 0.1, // ✓ Giảm temperature để có kết quả ổn định hơn
                    topK = 1, // ✓ Giảm topK để ưu tiên kết quả xác suất cao nhất
                    topP = 0.2, // ✓ Giảm topP để ưu tiên kết quả xác suất cao nhất
                    maxOutputTokens = 100, // ✓ Giảm còn 100 tokens để phản hồi nhanh hơn
                    stopSequences = new string[] { "Khách:", "User:" }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var url = $"{_geminiApiUrl}?key={_geminiApiKey}";

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // Timeout sau 5 giây
                    var response = await client.PostAsync(url, content, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        RecordApiSuccess();
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

                        var result = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
                        if (!string.IsNullOrEmpty(result))
                        {
                            // ✓ Post-process response - loại bỏ thông tin thừa
                            result = result.Replace("Tôi là trợ lý AI của StreamCart.", "")
                                          .Replace("Tôi là StreamCart AI.", "");
                            return result;
                        }

                        return "Tôi hiểu câu hỏi của anh/chị! 😊";
                    }

                    // Xử lý lỗi và fallback
                    throw new HttpRequestException($"Gemini API error: {response.StatusCode}");
                }
                catch (Exception)
                {
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(baseDelayMs);
                        continue;
                    }

                    RecordApiFailure();
                    throw;
                }
            }

            RecordApiFailure();
            throw new HttpRequestException("Gemini API unavailable after retries");
        }


        // Supporting classes
        private class UniversalContext
        {
            public ChatbotIntent Intent { get; set; }
            public Guid UserId { get; set; }
            public string SessionId { get; set; } = string.Empty;
            public List<ProductDto>? SuggestedProducts { get; set; }
            public List<ShopDto>? SuggestedShops { get; set; }
            public List<string>? Categories { get; set; }
            public Dictionary<string, object>? PriceRanges { get; set; }
            public string? ConversationHistory { get; set; }
        }

        private class UniversalChatbotIntent : ChatbotIntent
        {
            public List<string> ProductKeywords { get; set; } = new();
            public List<string> ShopKeywords { get; set; } = new();
            public List<string> CategoryKeywords { get; set; } = new();
            public bool RequiresProductInfo { get; set; }
            public bool RequiresShopInfo { get; set; }
        }

        private class GeminiResponse
        {
            public List<Candidate>? Candidates { get; set; }
        }

        private class Candidate
        {
            public Content? Content { get; set; }
        }

        private class Content
        {
            public List<Part>? Parts { get; set; }
        }

        private class Part
        {
            public string? Text { get; set; }
        }
    }
}