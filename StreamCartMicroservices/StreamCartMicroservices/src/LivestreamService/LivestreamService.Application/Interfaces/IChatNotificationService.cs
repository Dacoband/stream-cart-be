using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface IChatNotificationService
    {
        /// <summary>
        /// Notify all clients in livestream about new message
        /// </summary>
        Task NotifyLivestreamMessageAsync<T>(Guid livestreamId, T message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify chat room participants about new message
        /// </summary>
        Task NotifyChatRoomMessageAsync<T>(Guid chatRoomId, T message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify about message moderation
        /// </summary>
        Task NotifyMessageModerationAsync(Guid livestreamId, Guid messageId, bool isModerated, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify about message edit
        /// </summary>
        Task NotifyMessageEditAsync<T>(Guid chatRoomId, T message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify about messages marked as read
        /// </summary>
        Task NotifyMessagesReadAsync(Guid chatRoomId, Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify user typing status
        /// </summary>
        Task NotifyTypingStatusAsync(Guid chatRoomId, Guid userId, bool isTyping, CancellationToken cancellationToken = default);
    }
}