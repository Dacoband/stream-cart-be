using LivestreamService.Application.DTOs.Chat;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface ISignalRChatService
    {
        // ✅ EXISTING METHODS - giữ nguyên
        /// <summary>
        /// Sends a chat message to a specific chat room using SignalR
        /// </summary>
        Task SendMessageToChatRoomAsync(Guid chatRoomId, Guid senderId, string senderName, string message);

        /// <summary>
        /// Sends a chat message to a livestream chat using SignalR
        /// </summary>
        Task SendMessageToLivestreamAsync(Guid livestreamId, Guid senderId, string senderName, string message);

        /// <summary>
        /// Notifies when a user joins a chat room
        /// </summary>
        Task NotifyUserJoinedChatRoomAsync(Guid chatRoomId, Guid userId, string userName);

        /// <summary>
        /// Notifies when a user leaves a chat room
        /// </summary>
        Task NotifyUserLeftChatRoomAsync(Guid chatRoomId, Guid userId, string userName);

        /// <summary>
        /// Sends a chat message notification to specific users
        /// </summary>
        Task SendChatNotificationAsync(Guid recipientUserId, ChatMessageDTO message);

        // ✅ NEW METHODS - thêm các methods bị thiếu
        /// <summary>
        /// Notifies about the creation of a new chat room
        /// </summary>
        Task NotifyNewChatRoomAsync(Guid recipientId, Guid chatRoomId, object chatRoomInfo);

        /// <summary>
        /// Notifies when messages have been read in a chat room
        /// </summary>
        Task NotifyMessagesReadAsync(Guid recipientId, Guid chatRoomId, Guid readByUserId);

        /// <summary>
        /// Notifies about typing status in a chat room
        /// </summary>
        Task NotifyTypingStatusAsync(Guid chatRoomId, Guid userId, string userName, bool isTyping);

        /// <summary>
        /// Notifies about new chat messages to specific users
        /// </summary>
        Task NotifyNewChatMessageAsync(Guid userId, ChatMessageDTO message);

        /// <summary>
        /// Notifies about mentions in chat messages
        /// </summary>
        Task NotifyMentionAsync(Guid userId, ChatMessageDTO message, string mentionContext);

        /// <summary>
        /// Notifies about message edits in a chat room
        /// </summary>
        Task NotifyMessageEditAsync<T>(Guid chatRoomId, T message);

        /// <summary>
        /// Notifies about new livestream messages
        /// </summary>
        Task NotifyNewLivestreamMessageAsync(Guid userId, LivestreamChatDTO message);

        /// <summary>
        /// Notifies about message moderation in livestreams
        /// </summary>
        Task NotifyMessageModerationAsync(Guid livestreamId, Guid messageId, bool isModerated);
    }
}