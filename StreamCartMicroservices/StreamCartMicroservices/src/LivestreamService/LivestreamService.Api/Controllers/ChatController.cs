using LivestreamService.Application.Commands.Chat;
using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Queries.Chat;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Api.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<ChatController> logger)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo hoặc lấy chat room với shop
        /// </summary>
        [HttpPost("rooms")]
        [ProducesResponseType(typeof(ApiResponse<ChatRoomDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();
                var command = new CreateChatRoomCommand
                {
                    UserId = userId,
                    ShopId = request.ShopId,
                    RelatedOrderId = request.RelatedOrderId,
                    InitialMessage = request.InitialMessage
                };

                var result = await _mediator.Send(command);
                return Created($"/api/chat/rooms/{result.Id}",
                    ApiResponse<ChatRoomDTO>.SuccessResult(result, "Tạo chat room thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat room");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách chat rooms của user
        /// </summary>
        [HttpGet("rooms")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ChatRoomDTO>>), 200)]
        public async Task<IActionResult> GetChatRooms(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var query = new GetChatRoomsQuery
                {
                    UserId = userId,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    IsActive = isActive
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<PagedResult<ChatRoomDTO>>.SuccessResult(result, "Lấy danh sách chat rooms thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat rooms");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy chat room với shop cụ thể
        /// </summary>
        [HttpGet("rooms/shop/{shopId}")]
        [ProducesResponseType(typeof(ApiResponse<ChatRoomDTO>), 200)]
        public async Task<IActionResult> GetChatRoomWithShop(Guid shopId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var query = new GetChatRoomQuery
                {
                    UserId = userId,
                    ShopId = shopId
                };

                var result = await _mediator.Send(query);
                if (result == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Chat room không tồn tại"));
                }

                return Ok(ApiResponse<ChatRoomDTO>.SuccessResult(result, "Lấy chat room thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat room with shop");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Gửi tin nhắn trong chat room
        /// </summary>
        [HttpPost("messages")]
        [ProducesResponseType(typeof(ApiResponse<ChatMessageDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> SendMessage([FromBody] SendChatMessageDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();
                var command = new SendChatMessageCommand
                {
                    ChatRoomId = request.ChatRoomId,
                    SenderId = userId,
                    Content = request.Content,
                    MessageType = request.MessageType,
                    AttachmentUrl = request.AttachmentUrl
                };

                var result = await _mediator.Send(command);
                return Created($"/api/chat/messages/{result.Id}",
                    ApiResponse<ChatMessageDTO>.SuccessResult(result, "Gửi tin nhắn thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chat message");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy lịch sử tin nhắn trong chat room
        /// </summary>
        [HttpGet("rooms/{chatRoomId}/messages")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ChatMessageDTO>>), 200)]
        public async Task<IActionResult> GetChatMessages(
            Guid chatRoomId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var query = new GetChatMessagesQuery
                {
                    ChatRoomId = chatRoomId,
                    RequesterId = userId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<PagedResult<ChatMessageDTO>>.SuccessResult(result, "Lấy lịch sử tin nhắn thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat messages");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Chỉnh sửa tin nhắn
        /// </summary>
        [HttpPut("messages/{messageId}")]
        [ProducesResponseType(typeof(ApiResponse<ChatMessageDTO>), 200)]
        public async Task<IActionResult> EditMessage(Guid messageId, [FromBody] EditChatMessageDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();
                var command = new EditChatMessageCommand
                {
                    MessageId = messageId,
                    UserId = userId,
                    Content = request.Content
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<ChatMessageDTO>.SuccessResult(result, "Chỉnh sửa tin nhắn thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing chat message");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Đánh dấu tin nhắn đã đọc
        /// </summary>
        [HttpPatch("rooms/{chatRoomId}/mark-read")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> MarkMessagesAsRead(Guid chatRoomId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var command = new MarkMessagesAsReadCommand
                {
                    ChatRoomId = chatRoomId,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Đánh dấu đã đọc thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
    }
}