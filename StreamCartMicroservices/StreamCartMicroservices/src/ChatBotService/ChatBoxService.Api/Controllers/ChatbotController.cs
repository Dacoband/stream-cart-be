using ChatBoxService.Application.DTOs;
using ChatBoxService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using Shared.Common.Services.User;

namespace ChatBoxService.Api.Controllers
{
    [ApiController]
    [Route("api/chatbot")]
    [Authorize]
    public class ChatbotController : ControllerBase
    {
        private readonly IGeminiChatbotService _chatbotService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(
            IGeminiChatbotService chatbotService,
            ICurrentUserService currentUserService,
            ILogger<ChatbotController> logger)
        {
            _chatbotService = chatbotService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// Gửi tin nhắn cho chatbot và nhận phản hồi thân thiện
        /// </summary>
        [HttpPost("chat")]
        [ProducesResponseType(typeof(ApiResponse<ChatbotResponseDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> ChatWithBot([FromBody] ChatbotRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();
                _logger.LogInformation("User {UserId} is chatting with bot for shop {ShopId}", userId, request.ShopId);

                // Analyze message intent first
                var intent = await _chatbotService.AnalyzeMessageIntentAsync(request.CustomerMessage);

                // Generate appropriate response
                string botResponse;
                if (request.ProductId.HasValue)
                {
                    botResponse = await _chatbotService.GenerateProductResponseAsync(request.CustomerMessage, request.ProductId.Value);
                }
                else
                {
                    botResponse = await _chatbotService.GenerateResponseAsync(request.CustomerMessage, request.ShopId, request.ProductId);
                }

                // Create suggested actions based on intent
                var suggestedActions = GenerateSuggestedActions(intent, request.ShopId, request.ProductId);

                var response = new ChatbotResponseDTO
                {
                    BotResponse = botResponse,
                    Intent = intent.Intent,
                    RequiresHumanSupport = intent.Confidence < 0.6m || intent.Intent == "complaint",
                    SuggestedActions = suggestedActions,
                    GeneratedAt = DateTime.UtcNow,
                    ConfidenceScore = intent.Confidence
                };

                return Ok(ApiResponse<ChatbotResponseDTO>.SuccessResult(response, "Chatbot đã phản hồi thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in chatbot conversation for shop {ShopId}", request.ShopId);
                return BadRequest(ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi xử lý tin nhắn. Vui lòng thử lại sau."));
            }
        }

        /// <summary>
        /// Phân tích ý định của tin nhắn khách hàng
        /// </summary>
        [HttpPost("analyze-intent")]
        [ProducesResponseType(typeof(ApiResponse<ChatbotIntent>), 200)]
        public async Task<IActionResult> AnalyzeIntent([FromBody] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Tin nhắn không được để trống"));
            }

            try
            {
                var intent = await _chatbotService.AnalyzeMessageIntentAsync(message);
                return Ok(ApiResponse<ChatbotIntent>.SuccessResult(intent, "Phân tích ý định thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing message intent: {Message}", message);
                return BadRequest(ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi phân tích tin nhắn"));
            }
        }

        /// <summary>
        /// Lấy gợi ý sản phẩm dựa trên tin nhắn khách hàng
        /// </summary>
        [HttpPost("product-suggestions")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductSuggestion>>), 200)]
        public async Task<IActionResult> GetProductSuggestions([FromBody] ChatbotRequestDTO request)
        {
            try
            {
                // This would require additional AI processing to understand customer needs
                // and match with products. For now, return empty list with appropriate message
                var suggestions = new List<ProductSuggestion>();

                return Ok(ApiResponse<List<ProductSuggestion>>.SuccessResult(
                    suggestions,
                    "Tính năng gợi ý sản phẩm sẽ được cập nhật trong tương lai"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product suggestions");
                return BadRequest(ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi lấy gợi ý sản phẩm"));
            }
        }

        /// <summary>
        /// Test endpoint để kiểm tra kết nối Gemini API
        /// </summary>
        [HttpPost("test")]
        [AllowAnonymous]
        public async Task<IActionResult> TestChatbot([FromBody] string testMessage)
        {
            try
            {
                // Use a test shop ID for testing
                var testShopId = Guid.NewGuid();
                var response = await _chatbotService.GenerateResponseAsync(
                    testMessage ?? "Xin chào, tôi cần hỗ trợ",
                    testShopId);

                return Ok(new
                {
                    success = true,
                    response = response,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing chatbot");
                return Ok(new
                {
                    success = false,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        private List<SuggestedAction> GenerateSuggestedActions(ChatbotIntent intent, Guid shopId, Guid? productId)
        {
            var actions = new List<SuggestedAction>();

            switch (intent.Intent)
            {
                case "product_inquiry":
                    actions.Add(new SuggestedAction
                    {
                        Title = "Xem tất cả sản phẩm",
                        Action = "view_products",
                        Url = $"/shops/{shopId}/products"
                    });
                    break;

                case "price_question":
                    if (productId.HasValue)
                    {
                        actions.Add(new SuggestedAction
                        {
                            Title = "Xem chi tiết sản phẩm",
                            Action = "view_product_detail",
                            Url = $"/products/{productId}"
                        });
                    }
                    break;

                case "complaint":
                    actions.Add(new SuggestedAction
                    {
                        Title = "Liên hệ nhân viên hỗ trợ",
                        Action = "contact_support",
                        Url = $"/shops/{shopId}/contact"
                    });
                    break;

                default:
                    actions.Add(new SuggestedAction
                    {
                        Title = "Trở về trang chủ",
                        Action = "go_home",
                        Url = "/"
                    });
                    break;
            }

            return actions;
        }
    }
}