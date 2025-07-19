using LivestreamService.Application.Interfaces;
using LivestreamService.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Services
{
    public class ChatNotificationService : IChatNotificationService
    {
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly ILogger<ChatNotificationService> _logger;

        public ChatNotificationService(
            IHubContext<ChatHub> chatHubContext,
            ILogger<ChatNotificationService> logger)
        {
            _chatHubContext = chatHubContext;
            _logger = logger;
        }

        public async Task NotifyLivestreamMessageAsync<T>(Guid livestreamId, T message, CancellationToken cancellationToken = default)
        {
            try
            {
                await _chatHubContext.Clients.Group($"livestream_{livestreamId}")
                    .SendAsync("NewLivestreamMessage", message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying livestream message for livestream {LivestreamId}", livestreamId);
            }
        }

        public async Task NotifyChatRoomMessageAsync<T>(Guid chatRoomId, T message, CancellationToken cancellationToken = default)
        {
            try
            {
                await _chatHubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("NewChatMessage", message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying chat room message for room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task NotifyMessageModerationAsync(Guid livestreamId, Guid messageId, bool isModerated, CancellationToken cancellationToken = default)
        {
            try
            {
                await _chatHubContext.Clients.Group($"livestream_{livestreamId}")
                    .SendAsync("MessageModerated", new { MessageId = messageId, IsModerated = isModerated }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying message moderation for livestream {LivestreamId}", livestreamId);
            }
        }

        public async Task NotifyMessageEditAsync<T>(Guid chatRoomId, T message, CancellationToken cancellationToken = default)
        {
            try
            {
                await _chatHubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("MessageEdited", message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying message edit for room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task NotifyMessagesReadAsync(Guid chatRoomId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _chatHubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("MessagesMarkedAsRead", new { ChatRoomId = chatRoomId, UserId = userId }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying messages read for room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task NotifyTypingStatusAsync(Guid chatRoomId, Guid userId, bool isTyping, CancellationToken cancellationToken = default)
        {
            try
            {
                await _chatHubContext.Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("UserTyping", new { UserId = userId, IsTyping = isTyping }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying typing status for room {ChatRoomId}", chatRoomId);
            }
        }
    }
}