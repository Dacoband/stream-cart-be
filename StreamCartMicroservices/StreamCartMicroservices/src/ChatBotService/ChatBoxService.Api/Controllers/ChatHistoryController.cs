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
    [Route("api/chat-history")]
    [Authorize]
    public class ChatHistoryController : ControllerBase
    {
        private readonly IChatHistoryService _chatHistoryService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ChatHistoryController> _logger;

        public ChatHistoryController(
            IChatHistoryService chatHistoryService,
            ICurrentUserService currentUserService,
            ILogger<ChatHistoryController> logger)
        {
            _chatHistoryService = chatHistoryService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy lịch sử chat với shop
        /// </summary>
        [HttpGet("shop/{shopId}")]
        public async Task<IActionResult> GetChatHistory(
            Guid shopId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var request = new GetChatHistoryRequest
                {
                    UserId = userId,
                    ShopId = shopId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _chatHistoryService.GetChatHistoryAsync(request);
                return Ok(ApiResponse<ChatHistoryResponse>.SuccessResult(result, "Lấy lịch sử chat thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat history for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult("Lỗi khi lấy lịch sử chat"));
            }
        }

        /// <summary>
        /// Xóa lịch sử chat với shop
        /// </summary>
        [HttpDelete("shop/{shopId}")]
        public async Task<IActionResult> DeleteChatHistory(Guid shopId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                await _chatHistoryService.DeleteUserConversationsAsync(userId, shopId);

                return Ok(ApiResponse<object>.SuccessResult(null, "Xóa lịch sử chat thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chat history for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult("Lỗi khi xóa lịch sử chat"));
            }
        }

        /// <summary>
        /// Lấy context cuộc trò chuyện hiện tại
        /// </summary>
        [HttpGet("shop/{shopId}/context")]
        public async Task<IActionResult> GetConversationContext(Guid shopId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var context = await _chatHistoryService.GetConversationContextAsync(userId, shopId);

                return Ok(ApiResponse<string>.SuccessResult(context, "Lấy context thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation context for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult("Lỗi khi lấy context cuộc trò chuyện"));
            }
        }
    }
}