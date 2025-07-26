using LivestreamService.Application.Commands.Chat;
using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
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
        private readonly ILivekitService _livekitService;

        public ChatController(
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<ChatController> logger, ILivekitService livekitService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _livekitService = livekitService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo hoặc lấy chat room với shop
        /// </summary>
        [HttpPost("rooms")]
        [ProducesResponseType(typeof(ApiResponse<LiveKitChatRoomDTO>), 201)]
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

                // Tạo LiveKit chat room
                var livekitRoomName = await _livekitService.CreateChatRoomAsync(request.ShopId, userId);

                // Generate token cho customer
                var customerToken = await _livekitService.GenerateChatTokenAsync(
                    livekitRoomName,
                    $"customer-{userId}",
                    isShop: false);

                // Lưu thông tin chat room vào database (MongoDB)
                var command = new CreateChatRoomCommand
                {
                    UserId = userId,
                    ShopId = request.ShopId,
                    RelatedOrderId = request.RelatedOrderId,
                    InitialMessage = request.InitialMessage,
                    LiveKitRoomName = livekitRoomName, 
                    CustomerToken = customerToken,
                };

                var chatRoom = await _mediator.Send(command);

                var result = new LiveKitChatRoomDTO
                {
                    Id = chatRoom.Id,
                    UserId = chatRoom.UserId,
                    ShopId = chatRoom.ShopId,
                    LiveKitRoomName = livekitRoomName,
                    CustomerToken = customerToken,
                    IsActive = chatRoom.IsActive,
                    StartedAt = chatRoom.StartedAt,
                    ShopName = chatRoom.ShopName,
                    UserName = chatRoom.UserName
                };

                return Created($"/api/chat/rooms/{result.Id}",
                    ApiResponse<LiveKitChatRoomDTO>.SuccessResult(result, "Tạo chat room thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating LiveKit chat room");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
        [HttpGet("rooms/{chatRoomId}/shop-token")]
        [ProducesResponseType(typeof(ApiResponse<ShopChatTokenDTO>), 200)]
        public async Task<IActionResult> GetShopChatToken(Guid chatRoomId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                // Lấy thông tin chat room
                var query = new GetChatRoomQuery { ChatRoomId = chatRoomId, RequesterId = userId };
                var chatRoom = await _mediator.Send(query);

                if (chatRoom == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Chat room không tồn tại"));
                }

                // Tạo LiveKit room name từ chat room info
                var livekitRoomName = $"chat-shop-{chatRoom.ShopId}-customer-{chatRoom.UserId}";

                // Generate token cho shop
                var shopToken = await _livekitService.GenerateChatTokenAsync(
                    livekitRoomName,
                    $"shop-{chatRoom.ShopId}",
                    isShop: true);

                var result = new ShopChatTokenDTO
                {
                    ChatRoomId = chatRoomId,
                    LiveKitRoomName = livekitRoomName,
                    ShopToken = shopToken,
                    CustomerName = chatRoom.UserName
                };

                return Ok(ApiResponse<ShopChatTokenDTO>.SuccessResult(result, "Lấy shop token thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shop chat token");
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

                // Enhance với LiveKit room status
                foreach (var room in result.Items)
                {
                    var livekitRoomName = $"chat-shop-{room.ShopId}-customer-{room.UserId}";
                    room.IsLiveKitActive = await _livekitService.IsRoomActiveAsync(livekitRoomName);
                }

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
        /// <summary>
        /// Join livestream chat (sử dụng LiveKit room của livestream)
        /// </summary>
        [HttpPost("livestream/{livestreamId}/join")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamChatTokenDTO>), 200)]
        public async Task<IActionResult> JoinLivestreamChat(Guid livestreamId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var username = _currentUserService.GetUsername();

                // Lấy thông tin livestream để có LiveKit room name
                // Giả sử livestream có LiveKitRoomId
                var livestreamRoomName = $"livestream-{livestreamId}"; // Hoặc lấy từ database

                // Generate token cho viewer
                var viewerToken = await _livekitService.GenerateChatTokenAsync(
                    livestreamRoomName,
                    $"viewer-{userId}",
                    isShop: false);

                var result = new LivestreamChatTokenDTO
                {
                    LivestreamId = livestreamId,
                    LiveKitRoomName = livestreamRoomName,
                    ViewerToken = viewerToken,
                    UserId = userId,
                    Username = username
                };

                return Ok(ApiResponse<LivestreamChatTokenDTO>.SuccessResult(result, "Join livestream chat thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining livestream chat");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
    }
    public class LiveKitChatRoomDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ShopId { get; set; }
        public string LiveKitRoomName { get; set; } = string.Empty;
        public string CustomerToken { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime StartedAt { get; set; }
        public string? ShopName { get; set; }
        public string? UserName { get; set; }
    }

    public class ShopChatTokenDTO
    {
        public Guid ChatRoomId { get; set; }
        public string LiveKitRoomName { get; set; } = string.Empty;
        public string ShopToken { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
    }

    public class LivestreamChatTokenDTO
    {
        public Guid LivestreamId { get; set; }
        public string LiveKitRoomName { get; set; } = string.Empty;
        public string ViewerToken { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string? Username { get; set; }
    }
}