using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Services
{
    public class SignalRChatService : ISignalRChatService
    {
        private readonly IHubContext<SignalRChatHub> _hubContext;
        private readonly ILogger<SignalRChatService> _logger;

        public SignalRChatService(
            IHubContext<SignalRChatHub> hubContext,
            ILogger<SignalRChatService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendMessageToChatRoomAsync(Guid chatRoomId, Guid senderId, string senderName, string message)
        {
            try
            {
                await _hubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("ReceiveChatMessage", new
                    {
                        SenderId = senderId,
                        SenderName = senderName,
                        Message = message,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("SignalR message sent to chat room {ChatRoomId} from {SenderName}",
                    chatRoomId, senderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR message to chat room {ChatRoomId}", chatRoomId);
                throw;
            }
        }

        public async Task SendMessageToLivestreamAsync(Guid livestreamId, Guid senderId, string senderName, string message)
        {
            try
            {
                await _hubContext.Clients.Group($"livestream_{livestreamId}")
                    .SendAsync("ReceiveLivestreamMessage", new
                    {
                        SenderId = senderId,
                        SenderName = senderName,
                        Message = message,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("SignalR message sent to livestream {LivestreamId} from {SenderName}",
                    livestreamId, senderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR message to livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        public async Task NotifyUserJoinedChatRoomAsync(Guid chatRoomId, Guid userId, string userName)
        {
            try
            {
                await _hubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("UserJoined", new
                    {
                        UserId = userId,
                        UserName = userName,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("SignalR notification: User {UserName} joined chat room {ChatRoomId}",
                    userName, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR join notification for chat room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task NotifyUserLeftChatRoomAsync(Guid chatRoomId, Guid userId, string userName)
        {
            try
            {
                await _hubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("UserLeft", new
                    {
                        UserId = userId,
                        UserName = userName,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("SignalR notification: User {UserName} left chat room {ChatRoomId}",
                    userName, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR leave notification for chat room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task SendChatNotificationAsync(Guid recipientUserId, ChatMessageDTO message)
        {
            try
            {
                await _hubContext.Clients.User(recipientUserId.ToString())
                    .SendAsync("NewChatNotification", message);

                _logger.LogInformation("SignalR chat notification sent to user {UserId} for message {MessageId}",
                    recipientUserId, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR chat notification to user {UserId}", recipientUserId);
            }
        }
    }
}