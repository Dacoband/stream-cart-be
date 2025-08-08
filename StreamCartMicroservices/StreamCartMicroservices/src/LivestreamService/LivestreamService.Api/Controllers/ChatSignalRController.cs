using Livestreamservice.Application.Queries;
using LivestreamService.Application.Commands.Chat;
using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries.Chat;
using LivestreamService.Domain.Enums;
using LivestreamService.Infrastructure.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace LivestreamService.Api.Controllers
{
    [ApiController]
    [Route("api/chatsignalr")]
    [Authorize]
    public class ChatSignalRController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ChatSignalRController> _logger;
        private readonly IChatNotificationServiceSignalR _signalRChatService;

        public ChatSignalRController(
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<ChatSignalRController> logger,
            IChatNotificationServiceSignalR signalRChatService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
            _signalRChatService = signalRChatService;
        }

        #region Chat Rooms

        /// <summary>
        /// Lấy danh sách chat rooms của người dùng hiện tại
        /// </summary>
        [HttpGet("rooms")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ChatRoomDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetMyChatRooms(
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
                _logger.LogError(ex, "Error getting user chat rooms");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của chat room
        /// </summary>
        [HttpGet("rooms/{chatRoomId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ChatRoomDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetChatRoomById(Guid chatRoomId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var query = new GetChatRoomQuery { ChatRoomId = chatRoomId, RequesterId = userId };
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Chat room không tồn tại"));
                }

                return Ok(ApiResponse<ChatRoomDTO>.SuccessResult(result, "Lấy thông tin chat room thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat room {ChatRoomId}", chatRoomId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
        /// <summary>
        /// Lấy danh sách chat rooms của shop
        /// </summary>
        [HttpGet("shop-rooms")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ChatRoomDTO>>), 200)]
        public async Task<IActionResult> GetShopChatRooms(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var shopId = Guid.Parse(_currentUserService.GetShopId());
                var query = new GetShopChatRoomsQuery
                {
                    ShopId = shopId,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    IsActive = isActive
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<PagedResult<ChatRoomDTO>>.SuccessResult(result, "Lấy danh sách chat rooms của shop thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shop chat rooms");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
        /// <summary>
        /// Tạo chat room mới với shop qua SignalR
        /// </summary>
        [HttpPost("rooms")]
        [Authorize]
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
                var username = _currentUserService.GetUsername();

                // Create the chat room using the same command as the regular flow
                var command = new CreateChatRoomCommand
                {
                    UserId = userId,
                    ShopId = request.ShopId,
                    RelatedOrderId = request.RelatedOrderId,
                    InitialMessage = request.InitialMessage
                };

                var result = await _mediator.Send(command);

                // Notify about the new chat room creation
                await _signalRChatService.NotifyNewChatRoomAsync(
                    request.ShopId,
                    result.Id,
                    new
                    {
                        ChatRoomId = result.Id,
                        ShopId = request.ShopId,
                        UserId = userId,
                        UserName = username,
                        CreatedAt = DateTime.UtcNow
                    });

                return Created($"/api/chatsignalr/rooms/{result.Id}",
                    ApiResponse<ChatRoomDTO>.SuccessResult(result, "Tạo chat room SignalR thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SignalR chat room");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Tham gia chat room qua SignalR
        /// </summary>
        [HttpPost("rooms/{chatRoomId}/join")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ChatRoomDTO>), 200)]
        public async Task<IActionResult> JoinSignalRChatRoom(Guid chatRoomId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var username = _currentUserService.GetUsername();

                // Get chat room info
                var query = new GetChatRoomQuery { ChatRoomId = chatRoomId, RequesterId = userId };
                var chatRoom = await _mediator.Send(query);

                if (chatRoom == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Chat room không tồn tại"));
                }

                // 1. Mark messages as read when joining
                var markReadCommand = new MarkMessagesAsReadCommand
                {
                    ChatRoomId = chatRoomId,
                    UserId = userId
                };
                await _mediator.Send(markReadCommand);

                // 2. Notify via SignalR that user joined the room
                await _signalRChatService.NotifyUserJoinedChatRoomAsync(chatRoomId, userId, username);

                // 3. Let other participants know that messages were read
                Guid recipientId = userId == chatRoom.UserId ? chatRoom.ShopId : chatRoom.UserId;
                await _signalRChatService.NotifyMessagesReadAsync(recipientId, chatRoomId, userId);

                return Ok(ApiResponse<ChatRoomDTO>.SuccessResult(chatRoom, "Tham gia chat room SignalR thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining SignalR chat room");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Rời khỏi chat room (đánh dấu là inactive)
        /// </summary>
        [HttpPost("rooms/{chatRoomId}/leave")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> LeaveChatRoom(Guid chatRoomId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var username = _currentUserService.GetUsername();

                // Check if room exists and user has access
                var query = new GetChatRoomQuery { ChatRoomId = chatRoomId, RequesterId = userId };
                var chatRoom = await _mediator.Send(query);

                if (chatRoom == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Chat room không tồn tại"));
                }

                // Leave chat room
                var command = new LeaveChatRoomCommand
                {
                    ChatRoomId = chatRoomId,
                    UserId = userId
                };
                var result = await _mediator.Send(command);

                // Notify others that user left the room
                await _signalRChatService.SendMessageToChatRoomAsync(
                    chatRoomId,
                    userId,
                    username,
                    $"{username} đã rời khỏi cuộc trò chuyện");

                return Ok(ApiResponse<bool>.SuccessResult(result, "Rời khỏi chat room thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving chat room {ChatRoomId}", chatRoomId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        #endregion

        #region Chat Messages

        /// <summary>
        /// Lấy lịch sử tin nhắn trong chat room
        /// </summary>
        [HttpGet("rooms/{chatRoomId}/messages")]
        [Authorize]
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
                _logger.LogError(ex, "Error getting chat messages for room {ChatRoomId}", chatRoomId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Gửi tin nhắn qua SignalR
        /// </summary>
        [HttpPost("messages")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ChatMessageDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> SendSignalRMessage([FromBody] SendChatMessageDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();
                var username = _currentUserService.GetUsername();

                // 1. Process message via MediatR (same as regular SendMessage)
                var command = new SendChatMessageCommand
                {
                    ChatRoomId = request.ChatRoomId,
                    SenderId = userId,
                    Content = request.Content,
                    MessageType = request.MessageType,
                    AttachmentUrl = request.AttachmentUrl
                };

                var result = await _mediator.Send(command);

                // 2. Get chat room details to determine recipient
                var roomQuery = new GetChatRoomQuery { ChatRoomId = request.ChatRoomId, RequesterId = userId };
                var chatRoom = await _mediator.Send(roomQuery);

                if (chatRoom == null)
                {
                    _logger.LogWarning("Chat room {ChatRoomId} not found after sending message", request.ChatRoomId);
                    return BadRequest(ApiResponse<object>.ErrorResult("Chat room không tồn tại"));
                }

                // 3. Send via SignalR to the chat room
                await _signalRChatService.SendMessageToChatRoomAsync(
                    request.ChatRoomId,
                    userId,
                    username,
                    request.Content);

                // 4. Send notification to recipients
                // If sender is customer, notify shop owner
                if (userId == chatRoom.UserId && chatRoom.ShopId != Guid.Empty)
                {
                    try
                    {
                        // Notify shop about new customer message
                        await _signalRChatService.NotifyNewChatMessageAsync(chatRoom.ShopId, result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Non-critical error notifying shop {ShopId} about new message",
                            chatRoom.ShopId);
                    }
                }
                // If sender is shop, notify customer
                else if (chatRoom.UserId != userId)
                {
                    await _signalRChatService.NotifyNewChatMessageAsync(chatRoom.UserId, result);
                }

                // 5. Check for mentions and handle notification
                if (request.Content.Contains("@"))
                {
                    // Simple mention handling - can be expanded for more sophisticated detection
                    await _signalRChatService.NotifyMentionAsync(
                        chatRoom.UserId != userId ? chatRoom.UserId : chatRoom.ShopId,
                        result,
                        $"@{username} mentioned you: {request.Content}");
                }

                return Created($"/api/chat/messages/{result.Id}",
                    ApiResponse<ChatMessageDTO>.SuccessResult(result, "Gửi tin nhắn SignalR thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR chat message");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Chỉnh sửa tin nhắn chat qua SignalR
        /// </summary>
        [HttpPut("messages/{messageId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ChatMessageDTO>), 200)]
        public async Task<IActionResult> EditChatMessage(
            Guid messageId,
            [FromBody] EditChatMessageDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();

                // Process edit via MediatR
                var command = new EditChatMessageCommand
                {
                    MessageId = messageId,
                    UserId = userId,
                    Content = request.Content
                };

                var result = await _mediator.Send(command);

                // Get chat room details for notifications
                var getMessage = new GetChatMessageQuery1 { MessageId = messageId };
                var message = await _mediator.Send(getMessage);

                // Notify room participants about edit
                if (message != null)
                {
                    await _signalRChatService.NotifyMessageEditAsync(message.ChatRoomId, result);
                }

                return Ok(ApiResponse<ChatMessageDTO>.SuccessResult(result, "Chỉnh sửa tin nhắn thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing chat message {MessageId}", messageId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Đánh dấu tin nhắn đã đọc
        /// </summary>
        [HttpPatch("rooms/{chatRoomId}/mark-read")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> MarkMessagesAsRead(Guid chatRoomId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                // Get chat room details
                var query = new GetChatRoomQuery { ChatRoomId = chatRoomId, RequesterId = userId };
                var chatRoom = await _mediator.Send(query);

                if (chatRoom == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Chat room không tồn tại"));
                }

                // Mark messages as read
                var command = new MarkMessagesAsReadCommand
                {
                    ChatRoomId = chatRoomId,
                    UserId = userId
                };
                var result = await _mediator.Send(command);

                // Notify the other party that messages were read
                Guid recipientId = userId == chatRoom.UserId ? chatRoom.ShopId : chatRoom.UserId;
                await _signalRChatService.NotifyMessagesReadAsync(recipientId, chatRoomId, userId);

                return Ok(ApiResponse<bool>.SuccessResult(result, "Đánh dấu đã đọc thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read for room {ChatRoomId}", chatRoomId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy số lượng tin nhắn chưa đọc
        /// </summary>
        [HttpGet("unread-count")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<Dictionary<Guid, int>>), 200)]
        public async Task<IActionResult> GetUnreadMessageCount()
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var query = new GetUnreadMessagesCountQuery { UserId = userId };
                var result = await _mediator.Send(query);

                return Ok(ApiResponse<Dictionary<Guid, int>>.SuccessResult(result, "Lấy số lượng tin nhắn chưa đọc thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread message count");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đang gõ của người dùng
        /// </summary>
        [HttpPost("rooms/{chatRoomId}/typing")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> UpdateTypingStatus(
            Guid chatRoomId,
            [FromBody] TypingStatusDTO request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var username = _currentUserService.GetUsername();

                // Notify room participants about typing status
                await _signalRChatService.NotifyTypingStatusAsync(
                    chatRoomId,
                    userId,
                    username,
                    request.IsTyping);

                return Ok(ApiResponse<object>.SuccessResult(
                    new { Status = "Success" },
                    $"Cập nhật trạng thái {(request.IsTyping ? "đang gõ" : "dừng gõ")} thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating typing status");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Tìm kiếm tin nhắn trong chat room
        /// </summary>
        [HttpGet("rooms/{chatRoomId}/messages/search")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ChatMessageDTO>>), 200)]
        public async Task<IActionResult> SearchMessages(
            Guid chatRoomId,
            [FromQuery] string searchTerm,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Từ khóa tìm kiếm không được để trống"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();
                var query = new SearchChatMessagesQuery
                {
                    ChatRoomId = chatRoomId,
                    RequesterId = userId,
                    SearchTerm = searchTerm,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<PagedResult<ChatMessageDTO>>.SuccessResult(result, "Tìm kiếm tin nhắn thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching messages in room {ChatRoomId}", chatRoomId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        #endregion

        #region Livestream Chat

        /// <summary>
        /// Tham gia livestream chat qua SignalR
        /// </summary>
        [HttpPost("livestream/{livestreamId}/join")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<LivestreamChatDTO>), 200)]
        public async Task<IActionResult> JoinSignalRLivestreamChat(Guid livestreamId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var username = _currentUserService.GetUsername();

                // Get livestream info if it exists
                var livestreamQuery = new GetLivestreamByIdQuery { Id = livestreamId };
                var livestream = await _mediator.Send(livestreamQuery);

                if (livestream == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Livestream không tồn tại"));
                }

                // Create livestream chat DTO
                var result = new LivestreamChatDTO
                {
                    Id = Guid.NewGuid(),
                    LivestreamId = livestreamId,
                    SenderId = userId,
                    SenderName = username,
                    SenderType = "Viewer",
                    Message = $"{username} đã tham gia livestream",
                    MessageType = MessageType.System,
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // Notify others via SignalR that user joined the livestream
                await _signalRChatService.SendMessageToLivestreamAsync(
                    livestreamId,
                    userId,
                    username,
                    result.Message);

                // Notify livestream owner
                await _signalRChatService.NotifyNewLivestreamMessageAsync(
                    livestream.SellerId,
                    result);

                return Ok(ApiResponse<LivestreamChatDTO>.SuccessResult(result, "Tham gia livestream chat SignalR thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining SignalR livestream chat");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Gửi tin nhắn trong livestream qua SignalR
        /// </summary>
        [HttpPost("livestream/{livestreamId}/messages")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<LivestreamChatDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> SendLivestreamMessage(
            Guid livestreamId,
            [FromBody] SendLivestreamMessageDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();
                var username = _currentUserService.GetUsername();

                // Process message through MediatR
                var command = new SendLivestreamMessageCommand
                {
                    LivestreamId = livestreamId,
                    SenderId = userId,
                    Message = request.Message,
                    MessageType = request.MessageType,
                    ReplyToMessageId = !string.IsNullOrEmpty(request.ReplyToMessageId) &&
                                       Guid.TryParse(request.ReplyToMessageId, out var messageId) ? messageId : null
                };

                var result = await _mediator.Send(command);

                // Send via SignalR
                await _signalRChatService.SendMessageToLivestreamAsync(
                    livestreamId,
                    userId,
                    username,
                    request.Message);

                // Get livestream info to notify owner
                var livestreamQuery = new GetLivestreamByIdQuery { Id = livestreamId };
                var livestream = await _mediator.Send(livestreamQuery);

                if (livestream != null && livestream.SellerId != userId)
                {
                    // Notify livestream owner about new message
                    await _signalRChatService.NotifyNewLivestreamMessageAsync(
                        livestream.SellerId,
                        result);
                }

                // If replying to someone, also notify them
                if (command.ReplyToMessageId.HasValue)
                {
                    try
                    {
                        var originalMessageQuery = new GetLivestreamMessageByIdQuery
                        {
                            MessageId = command.ReplyToMessageId.Value
                        };
                        var originalMessage = await _mediator.Send(originalMessageQuery);

                        if (originalMessage != null && originalMessage.SenderId != userId)
                        {
                            await _signalRChatService.NotifyMentionAsync(
                                originalMessage.SenderId,
                                new ChatMessageDTO
                                {
                                    Id = result.Id,
                                    SenderName = username,
                                    Content = $"Trả lời tin nhắn của bạn: {result.Message}"
                                },
                                "Reply notification");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Non-critical error when notifying about reply to message");
                    }
                }

                return Created($"/api/livestream-chat/{result.Id}",
                    ApiResponse<LivestreamChatDTO>.SuccessResult(result, "Gửi tin nhắn livestream thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR livestream message");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy lịch sử chat của livestream
        /// </summary>
        [HttpGet("livestream/{livestreamId}/messages")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LivestreamChatDTO>>), 200)]
        public async Task<IActionResult> GetLivestreamChatHistory(
            Guid livestreamId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] bool includeModerated = false)
        {
            try
            {
                var query = new GetLivestreamChatHistoryQuery
                {
                    LivestreamId = livestreamId,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    IncludeModerated = includeModerated
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<PagedResult<LivestreamChatDTO>>.SuccessResult(result, "Lấy lịch sử chat livestream thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream chat history");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Kiểm duyệt tin nhắn trong livestream (chỉ dành cho chủ livestream)
        /// </summary>
        [HttpPatch("livestream/messages/{messageId}/moderate")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<LivestreamChatDTO>), 200)]
        public async Task<IActionResult> ModerateLivestreamMessage(
            Guid messageId,
            [FromBody] ModerateMessageDTO request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                // Get message info first
                var getMessageQuery = new GetLivestreamMessageByIdQuery { MessageId = messageId };
                var message = await _mediator.Send(getMessageQuery);

                if (message == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Tin nhắn không tồn tại"));
                }

                // Check if user is the livestream owner
                var livestreamQuery = new GetLivestreamByIdQuery { Id = message.LivestreamId };
                var livestream = await _mediator.Send(livestreamQuery);

                if (livestream == null || livestream.SellerId != userId)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResult("Bạn không có quyền kiểm duyệt tin nhắn này"));
                }

                // Moderate message
                var command = new ModerateLivestreamMessageCommand
                {
                    MessageId = messageId,
                    IsModerated = request.IsModerated,
                    ModeratorId = userId
                };

                var result = await _mediator.Send(command);

                // Notify about moderation via SignalR
                // Sửa dòng 793
                await _signalRChatService.NotifyMessageModerationAsync(
                    message.LivestreamId,
                    messageId,
                    request.IsModerated);
                return Ok(ApiResponse<LivestreamChatDTO>.SuccessResult(result, "Kiểm duyệt tin nhắn livestream thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating livestream message");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        #endregion

        #region Testing & Diagnostics

        /// <summary>
        /// Test gửi thông báo SignalR
        /// </summary>
        [HttpPost("test-notification")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> TestNotification([FromBody] TestNotificationDTO request)
        {
            try
            {
                if (request.UserId == Guid.Empty)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("UserId không hợp lệ"));
                }

                // Create test message
                var testMessage = new ChatMessageDTO
                {
                    Id = Guid.NewGuid(),
                    ChatRoomId = request.ChatRoomId ?? Guid.NewGuid(),
                    SenderUserId = request.SenderId ?? Guid.NewGuid(),
                    SenderName = request.SenderName ?? "Test Sender",
                    Content = request.Message ?? "Đây là tin nhắn test thông báo SignalR",
                    SentAt = DateTime.UtcNow,
                    MessageType = "Text"
                };

                // Send notification through SignalR
                await _signalRChatService.NotifyNewChatMessageAsync(
                    request.UserId,
                    testMessage);

                return Ok(ApiResponse<object>.SuccessResult(
                    new
                    {
                        Success = true,
                        Message = "Thông báo đã gửi thành công",
                        RecipientId = request.UserId,
                        SentAt = DateTime.UtcNow
                    },
                    "Gửi thông báo test thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test notification");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Kiểm tra kết nối SignalR
        /// </summary>
        [HttpGet("connection-status")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult CheckConnectionStatus()
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var username = _currentUserService.GetUsername();

                return Ok(ApiResponse<object>.SuccessResult(
                    new
                    {
                        IsConnected = true,
                        UserId = userId,
                        Username = username,
                        Timestamp = DateTime.UtcNow
                    },
                    "Kết nối SignalR đang hoạt động"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking SignalR connection status");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi kiểm tra kết nối SignalR"));
            }
        }
        /// <summary>
        /// Test SignalR connectivity và handshake
        /// </summary>
        [HttpGet("test-connectivity")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult TestConnectivity()
        {
            return Ok(ApiResponse<object>.SuccessResult(
                new
                {
                    SignalRHub = "/signalrchat",
                    WebSocketUrl = "wss://brightpa.me/signalrchat",
                    Status = "Available",
                    HandshakeInstructions = new
                    {
                        Step1 = "Connect to WebSocket URL",
                        Step2 = "Send handshake immediately: {\"protocol\":\"json\",\"version\":1}",
                        Step3 = "Wait for response: {}",
                        Step4 = "Then send commands"
                    },
                    SupportedMethods = new[]
                    {
                "JoinDirectChatRoom",
                "SendMessageToChatRoom",
                "SetTypingStatus"
                    },
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    Timestamp = DateTime.UtcNow
                },
                "SignalR connectivity test"));
        }

        /// <summary>
        /// Test authentication với token hiện tại
        /// </summary>
        [HttpGet("test-auth")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult TestAuth()
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var username = _currentUserService.GetUsername();
                var roles = _currentUserService.GetRoles();

                return Ok(ApiResponse<object>.SuccessResult(
                    new
                    {
                        UserId = userId,
                        Username = username,
                        Roles = roles,
                        TokenValid = true,
                        CanUseSignalR = true,
                        Claims = User.Claims.Select(c => new { c.Type, c.Value }).Take(10),
                        Timestamp = DateTime.UtcNow
                    },
                    "Authentication successful for SignalR"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Auth failed: {ex.Message}"));
            }
        }

        /// <summary>
        /// Test manual SignalR connection
        /// </summary>
        [HttpPost("test-manual-connection")]
        [AllowAnonymous]
        public async Task<IActionResult> TestManualConnection([FromBody] TestConnectionRequest request)
        {
            try
            {
                // Simulate một connection tới hub
                var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<SignalRChatHub>>();

                // Test gửi message đến tất cả clients
                await hubContext.Clients.All.SendAsync("TestMessage", new
                {
                    Message = "Test from manual connection",
                    Timestamp = DateTime.UtcNow,
                    TestId = request.TestId ?? Guid.NewGuid().ToString()
                });

                return Ok(ApiResponse<object>.SuccessResult(
                    new
                    {
                        TestId = request.TestId,
                        Message = "Manual test sent successfully",
                        Timestamp = DateTime.UtcNow
                    },
                    "Manual connection test successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Manual test failed: {ex.Message}"));
            }
        }

        #endregion
    }
    public class TestConnectionRequest
    {
        public string? TestId { get; set; }
        public string? Message { get; set; }
    }
    public class TypingStatusDTO
    {
        [Required]
        public bool IsTyping { get; set; }
    }

    public class TestNotificationDTO
    {
        [Required]
        public Guid UserId { get; set; }
        public Guid? ChatRoomId { get; set; }
        public Guid? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? Message { get; set; }
    }

    public class ModerateMessageDTO
    {
        [Required]
        public bool IsModerated { get; set; }
    }
}