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
        public async Task NotifyNewChatRoomAsync(Guid recipientId, Guid chatRoomId, object chatRoomInfo)
        {
            try
            {
                await _hubContext.Clients.User(recipientId.ToString())
                    .SendAsync("NewChatRoom", new
                    {
                        ChatRoomId = chatRoomId,
                        Info = chatRoomInfo,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("SignalR new chat room notification sent to user {UserId} for room {ChatRoomId}",
                    recipientId, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR new chat room notification to user {UserId}", recipientId);
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
        public async Task NotifyMessagesReadAsync(Guid recipientId, Guid chatRoomId, Guid readByUserId)
        {
            try
            {
                await _hubContext.Clients.User(recipientId.ToString())
                    .SendAsync("MessagesRead", new
                    {
                        ChatRoomId = chatRoomId,
                        ReadByUserId = readByUserId,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("SignalR messages read notification sent to user {UserId} for room {ChatRoomId}",
                    recipientId, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR messages read notification to user {UserId}", recipientId);
            }
        }
        public async Task NotifyTypingStatusAsync(Guid chatRoomId, Guid userId, string userName, bool isTyping)
        {
            try
            {
                await _hubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("TypingStatus", new
                    {
                        UserId = userId,
                        UserName = userName,
                        ChatRoomId = chatRoomId,
                        IsTyping = isTyping,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("SignalR typing status notification sent for user {UserName} in room {ChatRoomId}: {Status}",
                    userName, chatRoomId, isTyping ? "typing" : "stopped typing");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR typing status notification for user {UserId}", userId);
            }
        }
        public async Task NotifyNewChatMessageAsync(Guid userId, ChatMessageDTO message)
        {
            try
            {
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("NewChatMessage", new
                    {
                        MessageId = message.Id,
                        ChatRoomId = message.ChatRoomId,
                        SenderName = message.SenderName,
                        Content = TruncateMessage(message.Content, 100),
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("SignalR new chat message notification sent to user {UserId} for message {MessageId}",
                    userId, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR new chat message notification to user {UserId}", userId);
            }
        }

        public async Task NotifyMentionAsync(Guid userId, ChatMessageDTO message, string mentionContext)
        {
            try
            {
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("Mention", new
                    {
                        MessageId = message.Id,
                        ChatRoomId = message.ChatRoomId,
                        SenderName = message.SenderName,
                        Content = message.Content,
                        MentionContext = mentionContext,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("SignalR mention notification sent to user {UserId} from {SenderName}",
                    userId, message.SenderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR mention notification to user {UserId}", userId);
            }
        }
        public async Task NotifyMessageEditAsync<T>(Guid chatRoomId, T message)
        {
            try
            {
                await _hubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("MessageEdited", new
                    {
                        ChatRoomId = chatRoomId,
                        Message = message,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("SignalR message edit notification sent for chat room {ChatRoomId}", chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR message edit notification for chat room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task NotifyNewLivestreamMessageAsync(Guid userId, LivestreamChatDTO message)
        {
            try
            {
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("NewLivestreamMessage", new
                    {
                        MessageId = message.Id,
                        LivestreamId = message.LivestreamId,
                        SenderName = message.SenderName,
                        Content = TruncateMessage(message.Message, 100),
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("SignalR new livestream message notification sent to user {UserId} for message {MessageId}",
                    userId, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR new livestream message notification to user {UserId}", userId);
            }
        }
        public async Task NotifyMessageModerationAsync(Guid livestreamId, Guid messageId, bool isModerated)
        {
            try
            {
                await _hubContext.Clients.Group($"livestream_{livestreamId}")
                    .SendAsync("MessageModerated", new
                    {
                        LivestreamId = livestreamId,
                        MessageId = messageId,
                        IsModerated = isModerated,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("SignalR message moderation notification sent for livestream {LivestreamId}, message {MessageId}",
                    livestreamId, messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR message moderation notification for livestream {LivestreamId}", livestreamId);
            }
        }

        // ✅ HELPER METHOD
        private string TruncateMessage(string message, int maxLength)
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            return message.Length <= maxLength
                ? message
                : message.Substring(0, maxLength) + "...";
        }
    }
}