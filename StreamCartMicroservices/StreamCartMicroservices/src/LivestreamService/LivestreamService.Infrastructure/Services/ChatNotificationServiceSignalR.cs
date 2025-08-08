using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Services
{
    public class ChatNotificationServiceSignalR : IChatNotificationServiceSignalR
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<ChatNotificationServiceSignalR> _logger;

        public ChatNotificationServiceSignalR(
            IHubContext<NotificationHub> hubContext,
            ILogger<ChatNotificationServiceSignalR> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyNewChatMessageAsync(Guid userId, ChatMessageDTO message)
        {
            try
            {
                var notification = new
                {
                    Type = "NewChatMessage",
                    ChatRoomId = message.ChatRoomId,
                    SenderId = message.SenderUserId,
                    SenderName = message.SenderName,
                    Content = TruncateMessage(message.Content, 100),
                    Timestamp = DateTime.UtcNow,
                    MessageId = message.Id
                };

                await _hubContext.Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation("Sent chat message notification to user {UserId} for message {MessageId}",
                    userId, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chat message notification to user {UserId}", userId);
            }
        }

        public async Task NotifyNewLivestreamMessageAsync(Guid userId, LivestreamChatDTO message)
        {
            try
            {
                var notification = new
                {
                    Type = "NewLivestreamMessage",
                    LivestreamId = message.LivestreamId,
                    SenderId = message.SenderId,
                    SenderName = message.SenderName,
                    Content = TruncateMessage(message.Message, 100),
                    Timestamp = DateTime.UtcNow,
                    MessageId = message.Id
                };

                await _hubContext.Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation("Sent livestream message notification to user {UserId} for message {MessageId}",
                    userId, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending livestream message notification to user {UserId}", userId);
            }
        }

        public async Task NotifyUnreadMessagesAsync(Guid userId, int unreadCount, Guid chatRoomId, string senderName)
        {
            try
            {
                var notification = new
                {
                    Type = "UnreadMessages",
                    ChatRoomId = chatRoomId,
                    UnreadCount = unreadCount,
                    SenderName = senderName,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation("Sent unread messages notification to user {UserId}: {Count} messages in room {ChatRoomId}",
                    userId, unreadCount, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending unread messages notification to user {UserId}", userId);
            }
        }

        public async Task NotifyMentionAsync(Guid userId, ChatMessageDTO message, string mentionContext)
        {
            try
            {
                var notification = new
                {
                    Type = "Mention",
                    ChatRoomId = message.ChatRoomId,
                    SenderId = message.SenderUserId,
                    SenderName = message.SenderName,
                    Content = mentionContext,
                    Timestamp = DateTime.UtcNow,
                    MessageId = message.Id
                };

                await _hubContext.Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation("Sent mention notification to user {UserId} from {SenderName}",
                    userId, message.SenderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending mention notification to user {UserId}", userId);
            }
        }

        // Implement the missing methods from the interface

        public async Task NotifyNewChatRoomAsync(Guid recipientId, Guid chatRoomId, object chatRoomInfo)
        {
            try
            {
                var notification = new
                {
                    Type = "NewChatRoom",
                    ChatRoomId = chatRoomId,
                    Info = chatRoomInfo,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"user_{recipientId}")
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation("Sent new chat room notification to user {UserId} for room {ChatRoomId}",
                    recipientId, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending new chat room notification to user {UserId}", recipientId);
            }
        }

        public async Task NotifyMessagesReadAsync(Guid recipientId, Guid chatRoomId, Guid readByUserId)
        {
            try
            {
                var notification = new
                {
                    Type = "MessagesRead",
                    ChatRoomId = chatRoomId,
                    ReadByUserId = readByUserId,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"user_{recipientId}")
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation("Sent messages read notification to user {UserId} for room {ChatRoomId} by user {ReadByUserId}",
                    recipientId, chatRoomId, readByUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending messages read notification to user {UserId}", recipientId);
            }
        }

        public async Task NotifyTypingStatusAsync(Guid chatRoomId, Guid userId, string userName, bool isTyping)
        {
            try
            {
                var notification = new
                {
                    Type = "TypingStatus",
                    ChatRoomId = chatRoomId,
                    UserId = userId,
                    UserName = userName,
                    IsTyping = isTyping,
                    Timestamp = DateTime.UtcNow
                };

                // Send to all users in the chat room group except the user who is typing
                await _hubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("ReceiveTypingStatus", notification);

                _logger.LogInformation("Sent typing status notification for user {UserId} in room {ChatRoomId}: {Status}",
                    userId, chatRoomId, isTyping ? "typing" : "stopped typing");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing status notification for user {UserId}", userId);
            }
        }

        // Implement additional methods required by the IChatNotificationServiceSignalR interface
        public async Task SendMessageToChatRoomAsync(Guid chatRoomId, Guid senderId, string senderName, string message)
        {
            try
            {
                var chatMessage = new
                {
                    ChatRoomId = chatRoomId,
                    SenderId = senderId,
                    SenderName = senderName,
                    Content = message,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("ReceiveMessage", chatMessage);

                _logger.LogInformation("Sent message to chat room {ChatRoomId} from {SenderName}",
                    chatRoomId, senderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chat room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task SendMessageToLivestreamAsync(Guid livestreamId, Guid senderId, string senderName, string message)
        {
            try
            {
                var livestreamMessage = new
                {
                    LivestreamId = livestreamId,
                    SenderId = senderId,
                    SenderName = senderName,
                    Content = message,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"livestream_{livestreamId}")
                    .SendAsync("ReceiveLivestreamMessage", livestreamMessage);

                _logger.LogInformation("Sent message to livestream {LivestreamId} from {SenderName}",
                    livestreamId, senderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to livestream {LivestreamId}", livestreamId);
            }
        }

        public async Task NotifyUserJoinedChatRoomAsync(Guid chatRoomId, Guid userId, string userName)
        {
            try
            {
                var notification = new
                {
                    Type = "UserJoined",
                    ChatRoomId = chatRoomId,
                    UserId = userId,
                    UserName = userName,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("UserJoined", notification);

                _logger.LogInformation("Notified that user {UserName} joined chat room {ChatRoomId}",
                    userName, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying about user joining chat room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task NotifyUserLeftChatRoomAsync(Guid chatRoomId, Guid userId, string userName)
        {
            try
            {
                var notification = new
                {
                    Type = "UserLeft",
                    ChatRoomId = chatRoomId,
                    UserId = userId,
                    UserName = userName,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("UserLeft", notification);

                _logger.LogInformation("Notified that user {UserName} left chat room {ChatRoomId}",
                    userName, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying about user leaving chat room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task SendChatNotificationAsync(Guid recipientUserId, ChatMessageDTO message)
        {
            try
            {
                await _hubContext.Clients.Group($"user_{recipientUserId}")
                    .SendAsync("NewChatNotification", message);

                _logger.LogInformation("Sent chat notification to user {UserId} for message {MessageId}",
                    recipientUserId, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chat notification to user {UserId}", recipientUserId);
            }
        }

        private string TruncateMessage(string message, int maxLength)
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            return message.Length <= maxLength
                ? message
                : message.Substring(0, maxLength) + "...";
        }
        // Thêm phương thức NotifyMessageEditAsync<T>
        public async Task NotifyMessageEditAsync<T>(Guid chatRoomId, T message)
        {
            try
            {
                var notification = new
                {
                    Type = "MessageEdited",
                    ChatRoomId = chatRoomId,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("MessageEdited", notification);

                _logger.LogInformation("Sent message edit notification for chat room {ChatRoomId}",
                    chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message edit notification for chat room {ChatRoomId}",
                    chatRoomId);
            }
        }

        // Thêm phương thức NotifyMessageModerationAsync
        public async Task NotifyMessageModerationAsync(Guid livestreamId, Guid messageId, bool isModerated)
        {
            try
            {
                var notification = new
                {
                    Type = "MessageModerated",
                    LivestreamId = livestreamId,
                    MessageId = messageId,
                    IsModerated = isModerated,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"livestream_{livestreamId}")
                    .SendAsync("MessageModerated", notification);

                _logger.LogInformation("Sent message moderation notification for livestream {LivestreamId}, message {MessageId}, moderation status: {IsModerated}",
                    livestreamId, messageId, isModerated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message moderation notification for livestream {LivestreamId}",
                    livestreamId);
            }
        }
    }
}