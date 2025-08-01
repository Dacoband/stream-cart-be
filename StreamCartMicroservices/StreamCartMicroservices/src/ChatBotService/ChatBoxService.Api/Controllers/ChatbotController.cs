﻿using ChatBoxService.Application.DTOs;
using ChatBoxService.Application.Interfaces;
using ChatBoxService.Infrastructure.Services;
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
        private readonly IUniversalChatbotService _universalChatbotService;

        public ChatbotController(
            IGeminiChatbotService chatbotService,
            ICurrentUserService currentUserService,
            ILogger<ChatbotController> logger,
            IUniversalChatbotService universalChatbotService)
        {
            _chatbotService = chatbotService;
            _currentUserService = currentUserService;
            _logger = logger;
            _universalChatbotService = universalChatbotService;
        }

        /// <summary>
        /// Gửi tin nhắn cho chatbot và nhận phản hồi thân thiện
        /// </summary>
        [HttpPost("chat")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ChatbotResponseDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Chat([FromBody] ChatbotRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();
                _logger.LogInformation("Processing universal chat request for user {UserId}: {Message}",
                    userId, request.CustomerMessage);

                var response = await _universalChatbotService.GenerateUniversalResponseAsync(
                    request.CustomerMessage,
                    userId);

                return Ok(ApiResponse<ChatbotResponseDTO>.SuccessResult(
                    response,
                    "StreamCart AI đã phản hồi thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing universal chat request: {Message}", request.CustomerMessage);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Đã xảy ra lỗi khi xử lý yêu cầu"));
            }
        }
        /// <summary>
        /// 🔓 Chat Anonymous - Không cần đăng nhập (features hạn chế)
        /// </summary>
        [HttpPost("chat/anonymous")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ChatbotResponseDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> ChatAnonymous([FromBody] ChatbotRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                _logger.LogInformation("🔓 Processing anonymous chat request: {Message}", request.CustomerMessage);

                // Sử dụng anonymous user ID
                var anonymousUserId = Guid.Empty;

                var response = await _universalChatbotService.GenerateUniversalResponseAsync(
                    request.CustomerMessage,
                    anonymousUserId);

                // Giới hạn features cho anonymous users
                if (response.ShopSuggestions?.Count > 2)
                {
                    response.ShopSuggestions = response.ShopSuggestions.Take(2).ToList();
                }
                if (response.ProductSuggestions?.Count > 3)
                {
                    response.ProductSuggestions = response.ProductSuggestions.Take(3).ToList();
                }

                return Ok(ApiResponse<ChatbotResponseDTO>.SuccessResult(
                    response,
                    "StreamCart AI đã phản hồi thành công (Anonymous mode)"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing anonymous chat request: {Message}", request.CustomerMessage);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Đã xảy ra lỗi khi xử lý yêu cầu"));
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
        public async Task<IActionResult> TestChatbot([FromBody] TestChatbotRequest request)
        {
            try
            {
                var testUserId = Guid.NewGuid();
                var testShopId = request?.ShopId ?? Guid.NewGuid();
                var testMessage = request?.Message ?? "Xin chào, shop có sản phẩm gì hay không?";

                // ✅ AI tự phân tích - không cần ProductId
                var response = await _chatbotService.GenerateResponseAsync(
                    testMessage,
                    testShopId,
                    testUserId,
                    productId: null); // ✅ Luôn null

                return Ok(new
                {
                    success = true,
                    response = response,
                    message = "AI đã tự động phân tích và trả lời",
                    shopId = testShopId,
                    userId = testUserId,
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
        /// <summary>
        /// Gửi tin nhắn tới dịch vụ AI bên ngoài và nhận phản hồi
        /// </summary>
        [HttpPost("chatAI")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AIChatResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> ChatWithAI([FromBody] AIChatRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId().ToString();
                _logger.LogInformation("Processing AI chat request for user {UserId}: {Message}",
                    userId, request.Message);

                var aiChatService = HttpContext.RequestServices.GetRequiredService<IAIChatService>();
                var response = await aiChatService.SendMessageAsync(request.Message, userId);

                return Ok(ApiResponse<AIChatResponse>.SuccessResult(
                    response,
                    "AI đã phản hồi thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI chat request: {Message}", request.Message);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Đã xảy ra lỗi khi xử lý yêu cầu"));
            }
        }

        /// <summary>
        /// Lấy lịch sử chat từ dịch vụ AI
        /// </summary>
        [HttpGet("chat/history")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AIChatHistoryResponse>), 200)]
        public async Task<IActionResult> GetAIChatHistory()
        {
            try
            {
                var userId = _currentUserService.GetUserId().ToString();
                _logger.LogInformation("Getting AI chat history for user {UserId}", userId);

                var aiChatService = HttpContext.RequestServices.GetRequiredService<IAIChatService>();
                var history = await aiChatService.GetChatHistoryAsync(userId);

                return Ok(ApiResponse<AIChatHistoryResponse>.SuccessResult(
                    history,
                    "Lấy lịch sử chat AI thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI chat history");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Đã xảy ra lỗi khi lấy lịch sử chat AI"));
            }
        }
        // ✅ THÊM DTO cho test
        public class TestChatbotRequest
        {
            public string? Message { get; set; }
            public Guid? ShopId { get; set; }
        }
        
    }
}