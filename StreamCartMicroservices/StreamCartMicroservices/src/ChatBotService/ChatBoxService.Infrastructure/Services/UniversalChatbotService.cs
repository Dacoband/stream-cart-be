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
        private readonly ICachingService _cachingService;

        // ✅ Circuit breaker state
        private static int _consecutiveFailures = 0;
        private static DateTime _lastFailureTime = DateTime.MinValue;
        private const int MAX_FAILURES = 3;
        private const int CIRCUIT_BREAKER_TIMEOUT_MINUTES = 5;

        public UniversalChatbotService(
            IHttpClientFactory httpClientFactory,
            IProductServiceClient productServiceClient,
            IShopServiceClient shopServiceClient,
            IConfiguration configuration,
            ILogger<UniversalChatbotService> logger,
            IChatHistoryService chatHistoryService,
            ICachingService cachingService)
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

                // ✅ Special handling for iPhone shop search
                if ((lowerMessage.Contains("có shop nào bán") || lowerMessage.Contains("shop nào bán"))
                    && lowerMessage.Contains("iphone"))
                {
                    _logger.LogInformation("🎯 DETECTED EXACT PATTERN: có shop nào bán iPhone");
                    return await HandleiPhoneShopSearchAsync(customerMessage, userId);
                }

                _logger.LogInformation("🤖 Processing universal chatbot request for user {UserId}: {Message}",
                    userId, customerMessage);

                // 1. Analyze message intent
                var intent = await AnalyzeUniversalIntentAsync(customerMessage);
                _logger.LogInformation("🎯 Analyzed universal intent: {Intent} with confidence {Confidence}",
                    intent.Intent, intent.Confidence);

                // 2. Fast path for simple intents
                if (intent.Intent == "greeting" || intent.Intent == "thanks")
                {
                    return await HandleSimpleIntentAsync(intent, customerMessage, userId);
                }

                // 3. Check FAQ matches
                if (intent.Intent != "product_search" && intent.Confidence < 0.8m)
                {
                    var faqResponse = CheckFAQMatch(customerMessage);
                    if (faqResponse != null)
                    {
                        if (userId != Guid.Empty)
                        {
                            await SaveUniversalConversationAsync(userId, customerMessage, faqResponse);
                        }
                        return faqResponse;
                    }
                }

                // 4. Build context and generate response
                var context = await BuildUniversalContextAsync(customerMessage, intent, userId);

                string aiResponseText;
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

                // 5. Build final response
                var chatbotResponse = await BuildChatbotResponseAsync(aiResponseText, intent, context);

                // 6. Save conversation history
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
                return CreateErrorResponse();
            }
        }

        public async Task<ChatbotIntent> AnalyzeUniversalIntentAsync(string customerMessage)
        {
            try
            {
                // Try offline analysis first
                var offlineIntent = AnalyzeIntentOffline(customerMessage);

                if (offlineIntent.Confidence >= 0.8m)
                {
                    _logger.LogInformation("✅ High confidence offline intent analysis: {Intent} ({Confidence})",
                        offlineIntent.Intent, offlineIntent.Confidence);
                    return offlineIntent;
                }

                // Use AI only when offline is not confident and circuit breaker is closed
                if (!IsCircuitBreakerOpen() && offlineIntent.Confidence < 0.7m)
                {
                    try
                    {
                        var systemPrompt = @"Phân tích ngắn gọn ý định tin nhắn tiếng Việt:

INTENT: greeting, product_search, shop_search, price_inquiry, recommendation, thanks, general_question

JSON: {""intent"": ""..."", ""confidence"": 0.8}

Tin nhắn: """ + customerMessage + @"""";

                        var response = await CallGeminiAPIAsync(systemPrompt, customerMessage);

                        var aiIntent = JsonSerializer.Deserialize<ChatbotIntent>(response);
                        if (aiIntent != null && aiIntent.Confidence > offlineIntent.Confidence)
                        {
                            _logger.LogInformation("🤖 AI intent analysis better than offline: {Intent} ({Confidence})",
                                aiIntent.Intent, aiIntent.Confidence);
                            return aiIntent;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ AI intent analysis failed, using offline result");
                    }
                }

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

        #region Private Helper Methods

        private async Task<ChatbotResponseDTO> HandleiPhoneShopSearchAsync(string customerMessage, Guid userId)
        {
            var shops = await SearchShopsUniversalAsync(new List<string> { "iphone" });

            string response;
            if (shops?.Any() == true)
            {
                response = "🏪 **Các shop bán iPhone trên StreamCart:**\n\n";
                foreach (var shop in shops.Take(5))
                {
                    response += $"• **{shop.ShopName}** - Cửa hàng uy tín\n";
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
                SuggestedActions = GenerateUniversalSuggestedActions(new ChatbotIntent { Intent = "shop_search" }),
                ShopSuggestions = ConvertShopsToSuggestions(shops?.Take(5).ToList() ?? new List<ShopDto>(), new ChatbotIntent { Intent = "shop_search" })
            };

            if (userId != Guid.Empty)
            {
                await SaveUniversalConversationAsync(userId, customerMessage, directResponse);
            }

            return directResponse;
        }

        private async Task<ChatbotResponseDTO> HandleSimpleIntentAsync(ChatbotIntent intent, string customerMessage, Guid userId)
        {
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

            if (userId != Guid.Empty)
            {
                await SaveUniversalConversationAsync(userId, customerMessage, fastResponse);
            }

            return fastResponse;
        }

        private ChatbotResponseDTO? CheckFAQMatch(string customerMessage)
        {
            var faqResponse = LocalTrainingData.FindBestMatchingFAQ(customerMessage, out bool isGoodFaqMatch);

            if (isGoodFaqMatch)
            {
                _logger.LogInformation("📚 Found good FAQ match for message");

                return new ChatbotResponseDTO
                {
                    BotResponse = faqResponse + "\n\n" + LocalTrainingData.GetRandomShoppingTip(),
                    Intent = "general_question",
                    ConfidenceScore = 0.95m,
                    RequiresHumanSupport = false,
                    SuggestedActions = GenerateUniversalSuggestedActions(new ChatbotIntent { Intent = "general_question" }),
                    GeneratedAt = DateTime.UtcNow
                };
            }

            return null;
        }

        private bool ShouldUseFallbackResponse(ChatbotIntent intent, UniversalContext context)
        {
            var simpleFallbackIntents = new[] { "greeting", "thanks", "general_question" };

            if (simpleFallbackIntents.Contains(intent.Intent))
            {
                _logger.LogInformation("📝 Intent {Intent} can use simple fallback", intent.Intent);
                return true;
            }

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

        private ChatbotIntent AnalyzeIntentOffline(string customerMessage)
        {
            var message = customerMessage.ToLower().Trim();

            _logger.LogInformation("🔍 Using enhanced offline intent analysis for: {Message}",
                customerMessage.Substring(0, Math.Min(30, customerMessage.Length)));

            // Check shop search patterns first
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

                var productKeywords = ExtractProductKeywordsFromShopSearch(message);

                _logger.LogInformation("🔑 Extracted product keywords from shop search: {Keywords}",
                    string.Join(", ", productKeywords));

                return new ChatbotIntent
                {
                    Intent = "shop_search",
                    Category = "shopping",
                    Confidence = 0.95m,
                    Keywords = productKeywords.Any() ? productKeywords : new List<string> { "sản phẩm" }
                };
            }

            // Use intent patterns for other cases
            var intent = IntentPatterns.DetectIntent(message, out decimal confidence);

            // Check FAQ matches
            if (intent != "product_search" && intent != "shop_search")
            {
                var faqMatchResponse = LocalTrainingData.FindBestMatchingFAQ(message, out bool isFaqMatch);

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

            // Create keywords based on intent
            var keywords = intent switch
            {
                "product_search" => ExtractKeywords(message, new[] {
                    "điện thoại", "laptop", "máy tính", "tablet", "ipad", "samsung", "iphone",
                    "giày", "quần áo", "thời trang", "áo", "quần", "váy", "đầm",
                    "mỹ phẩm", "son", "kem", "sữa rửa mặt", "serum",
                    "đồng hồ", "túi xách", "balo", "kính"
                }),
                "price_inquiry" => ExtractKeywords(message, new[] {
                    "điện thoại", "laptop", "máy tính", "quần áo", "giày"
                }),
                _ => message.Split(' ')
                    .Where(w => w.Length > 3 && !new[] { "tìm", "mua", "cần", "muốn", "bao nhiêu", "giá" }.Contains(w))
                    .Take(3)
                    .ToList()
            };

            return new ChatbotIntent
            {
                Intent = intent,
                Category = intent == "greeting" || intent == "thanks" ? "customer_service" : "shopping",
                Confidence = confidence,
                Keywords = keywords.Any() ? keywords : new List<string> { "sản phẩm" }
            };
        }

        private List<string> ExtractProductKeywordsFromShopSearch(string message)
        {
            var productKeywords = new[] {
                "điện thoại", "laptop", "giày", "áo", "quần", "túi", "kính",
                "đồng hồ", "máy tính", "tablet", "earphone", "airpod", "phone", "iphone",
                "thời trang", "mỹ phẩm", "sách", "đồ chơi", "đồ gia dụng", "samsung"
            };

            var foundKeywords = new List<string>();

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

                        if (productName.Contains("iphone") || productName.Contains("điện thoại"))
                        {
                            return foundKeywords;
                        }
                    }
                }
            }

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
                var words = message.Split(' ')
                    .Where(w => w.Length > 2 && !new[] { "tìm", "mua", "cần", "muốn", "có", "không", "gì", "là", "của" }.Contains(w))
                    .Take(3)
                    .ToList();
                keywords.AddRange(words);
            }

            return keywords;
        }

        private string GetFallbackResponse(ChatbotIntent intent, UniversalContext context)
        {
            return intent.Intent switch
            {
                "greeting" => ResponseTemplates.GetRandomResponse("greeting"),
                "thanks" => ResponseTemplates.GetRandomResponse("thanks"),
                "product_search" => GenerateProductSearchFallback(context),
                "shop_search" => GenerateShopSearchFallback(context),
                "price_inquiry" => GeneratePriceInquiryFallback(context),
                _ => GenerateGeneralFallback(context)
            };
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
                var shopResponse = "🏪 **Cửa hàng có bán sản phẩm bạn tìm trên StreamCart:**\n\n";

                foreach (var s in context.SuggestedShops.Take(5))
                {
                    shopResponse += $"• **{s.ShopName}** - Cửa hàng uy tín\n";
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

        private string GenerateGeneralFallback(UniversalContext context)
        {
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
                           $"• {s.ShopName} - Cửa hàng uy tín"));
            }

            return ResponseTemplates.GetRandomResponse("fallback");
        }

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

                // Handle different intents
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

                        context.SuggestedProducts = await GetTrendingProductsAsync();
                        context.SuggestedShops = await GetPopularShopsAsync();
                        context.Categories = await GetPopularCategoriesAsync();
                        break;
                }

                // Get conversation history if user is logged in
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

                return await _cachingService.GetOrCreateAsync(cacheKey, async () =>
                {
                    _logger.LogInformation("🔍 Searching products universally with keywords: {Keywords}",
                        string.Join(", ", keywords));

                    var allProducts = new List<ProductDto>();

                    // Get active shops
                    var activeShops = await GetPopularShopsAsync();
                    var topShops = activeShops.Take(5).ToList();

                    if (!topShops.Any())
                    {
                        _logger.LogWarning("⚠️ No active shops found to search products");
                        return allProducts;
                    }

                    // Search products from each shop in parallel
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
                                var matchingProducts = shopProducts
                                    .Where(p => keywords.Any(k =>
                                        p.ProductName?.Contains(k, StringComparison.OrdinalIgnoreCase) == true))
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

                    foreach (var result in searchResults)
                    {
                        allProducts.AddRange(result);
                    }

                    var sortedProducts = allProducts
                        .OrderByDescending(p => keywords.Count(k =>
                            p.ProductName?.Contains(k, StringComparison.OrdinalIgnoreCase) == true))
                        .ThenBy(p => p.FinalPrice)
                        .Take(15)
                        .ToList();

                    _logger.LogInformation("✅ Universal product search completed: {Count} products found",
                        sortedProducts.Count);

                    return sortedProducts;
                }, TimeSpan.FromMinutes(15));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in universal product search");
                return new List<ProductDto>();
            }
        }

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

                // Find products containing keywords
                var allProducts = await SearchProductsUniversalAsync(keywords);

                if (allProducts?.Any() == true)
                {
                    var shopIds = allProducts.Select(p => p.ShopId).Distinct().ToList();

                    _logger.LogInformation("🏪 Found {ProductCount} products from {ShopCount} different shops",
                        allProducts.Count, shopIds.Count);

                    foreach (var shopId in shopIds.Take(10))
                    {
                        try
                        {
                            var shopInfo = await _shopServiceClient.GetShopByIdAsync(shopId);
                            if (shopInfo != null)
                            {
                                var shopDto = MapShopInfoToShopDto(shopInfo);
                                matchingShops.Add(shopDto);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "⚠️ Error getting shop {ShopId}", shopId);
                        }
                    }
                }

                var uniqueShops = matchingShops
                    .GroupBy(s => s.Id)
                    .Select(g => g.First())
                    .Take(8)
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

        private async Task<List<ShopDto>> GetShopsWithProductsAsync(List<ProductDto> products)
        {
            try
            {
                if (products == null || !products.Any())
                    return new List<ShopDto>();

                var shopIds = products.Select(p => p.ShopId).Distinct().ToList();
                var shops = new List<ShopDto>();

                _logger.LogInformation("🏪 Getting shop details for {Count} shops", shopIds.Count);

                foreach (var shopId in shopIds.Take(10))
                {
                    try
                    {
                        var shopInfo = await _shopServiceClient.GetShopByIdAsync(shopId);
                        if (shopInfo != null)
                        {
                            var shopDto = MapShopInfoToShopDto(shopInfo);
                            shops.Add(shopDto);
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

        private async Task<List<ShopDto>> GetPopularShopsAsync()
        {
            try
            {
                _logger.LogInformation("🔥 Getting popular shops");

                // Since we don't have GetShopsByStatusAsync, we'll return empty list
                // This should be implemented when the proper interface is available
                return new List<ShopDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting popular shops");
                return new List<ShopDto>();
            }
        }

        private async Task<List<string>> GetPopularCategoriesAsync()
        {
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

        private async Task<List<ProductDto>> GetTrendingProductsAsync()
        {
            try
            {
                _logger.LogInformation("🔥 Getting trending products across platform");

                var trendingProducts = new List<ProductDto>();
                var popularShops = await GetPopularShopsAsync();

                foreach (var shop in popularShops.Take(3))
                {
                    try
                    {
                        var shopProducts = await _productServiceClient.GetProductsByShopIdAsync(shop.Id, activeOnly: true);
                        if (shopProducts?.Any() == true)
                        {
                            var topProducts = shopProducts
                                .Where(p => p.StockQuantity > 0)
                                .OrderByDescending(p => p.QuantitySold)
                                .ThenBy(p => p.FinalPrice)
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

                var result = trendingProducts.Take(10).ToList();
                _logger.LogInformation("✅ Got {Count} trending products", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting trending products");
                return new List<ProductDto>();
            }
        }

        private async Task<List<ProductDto>> SearchProductsByKeywordsAsync(List<string> keywords)
        {
            return await SearchProductsUniversalAsync(keywords);
        }

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

        private ShopDto MapShopInfoToShopDto(ShopInfoDTO shopInfo)
        {
            return new ShopDto
            {
                Id = shopInfo.Id,
                ShopName = shopInfo.ShopName,
                Description = shopInfo.Description,
                LogoUrl = shopInfo.LogoUrl,
                Address = shopInfo.Address,
                Phone = shopInfo.Phone,
                Email = shopInfo.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = null,
                Rating = 4.5m,
                TotalReviews = 0,
                TotalProducts = 0 // ShopInfoDTO doesn't have this field
            };
        }

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

            // Add specific context based on intent
            switch (intent.Intent)
            {
                case "product_search":
                    if (context.SuggestedProducts?.Any() == true)
                    {
                        prompt.AppendLine("\n🛍️ SẢN PHẨM ĐƯỢC TÌM THẤY TRÊN STREAMCART:");
                        foreach (var product in context.SuggestedProducts.Take(6))
                        {
                            var stockStatus = product.StockQuantity > 0 ? $"Còn {product.StockQuantity}" : "Hết hàng";
                            prompt.AppendLine($"• {product.ProductName} - {product.FinalPrice:N0}đ ({stockStatus})");
                        }
                    }
                    break;

                case "shop_search":
                    if (context.SuggestedShops?.Any() == true)
                    {
                        prompt.AppendLine("\n🏪 CỬA HÀNG UY TÍN TRÊN STREAMCART:");
                        foreach (var shop in context.SuggestedShops.Take(5))
                        {
                            prompt.AppendLine($"• {shop.ShopName} - Cửa hàng uy tín");
                        }
                    }
                    break;
            }

            prompt.AppendLine("\n💡 HÃY TRẢ LỜI DỰA TRÊN THÔNG TIN SẢN PHẨM THẬT VÀ GỢI Ý THÔNG MINH:");

            return prompt.ToString();
        }

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
                ShopSuggestions = ConvertShopsToSuggestions(context.SuggestedShops ?? new List<ShopDto>(), intent),
                ProductSuggestions = ConvertProductsToSuggestions(context.SuggestedProducts ?? new List<ProductDto>(), intent),
                GeneratedAt = DateTime.UtcNow
            };

            return response;
        }

        private List<ShopSuggestion> ConvertShopsToSuggestions(List<ShopDto> shops, ChatbotIntent intent)
        {
            return shops.Take(5).Select(s => new ShopSuggestion
            {
                ShopId = s.Id,
                ShopName = s.ShopName,
                ProductCount = s.TotalProducts,
                Rating = s.Rating,
                Location = s.Address ?? "Việt Nam",
                LogoUrl = s.LogoUrl,
                Description = s.Description,
                ReasonForSuggestion = GetShopSuggestionReason(intent, s)
            }).ToList();
        }

        private List<ProductSuggestion> ConvertProductsToSuggestions(List<ProductDto> products, ChatbotIntent intent)
        {
            return products.Take(8).Select(p => new ProductSuggestion
            {
                ProductId = p.Id,
                ProductName = p.ProductName,
                Price = p.FinalPrice,
                ImageUrl = p.PrimaryImageUrl,
                ShopName = "Shop trên StreamCart",
                ReasonForSuggestion = GetProductSuggestionReason(intent, p)
            }).ToList();
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
                    break;

                case "product_search":
                    actions.Add(new SuggestedAction
                    {
                        Title = "🔍 Tìm kiếm nâng cao",
                        Action = "advanced_search",
                        Url = "/search/advanced"
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
            }

            actions.Add(new SuggestedAction
            {
                Title = "💬 Hỗ trợ trực tiếp",
                Action = "contact_support",
                Url = "/support"
            });

            return actions;
        }

        private async Task<string> GetUniversalConversationHistoryAsync(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return "";

                _logger.LogInformation("📚 Getting universal conversation history for user {UserId}", userId);

                var universalShopId = Guid.Empty;

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

                var universalShopId = Guid.Empty;
                var sessionId = $"universal-session-{DateTime.UtcNow:yyyyMMdd}";

                var conversation = await _chatHistoryService.GetOrCreateConversationAsync(
                    userId,
                    universalShopId,
                    sessionId);

                await _chatHistoryService.AddMessageToConversationAsync(
                    conversation.ConversationId,
                    customerMessage,
                    "User",
                    response.Intent,
                    response.ConfidenceScore);

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

        private async Task<string> CallGeminiAPIAsync(string systemPrompt, string userMessage)
        {
            var client = _httpClientFactory.CreateClient();
            const int maxRetries = 1;
            const int baseDelayMs = 2000;

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
                    temperature = 0.1,
                    topK = 1,
                    topP = 0.2,
                    maxOutputTokens = 100,
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
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    var response = await client.PostAsync(url, content, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        RecordApiSuccess();
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

                        var result = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
                        if (!string.IsNullOrEmpty(result))
                        {
                            result = result.Replace("Tôi là trợ lý AI của StreamCart.", "")
                                          .Replace("Tôi là StreamCart AI.", "");
                            return result;
                        }

                        return "Tôi hiểu câu hỏi của anh/chị! 😊";
                    }

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

        #endregion

        #region Supporting Classes

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

        #endregion
    }
}