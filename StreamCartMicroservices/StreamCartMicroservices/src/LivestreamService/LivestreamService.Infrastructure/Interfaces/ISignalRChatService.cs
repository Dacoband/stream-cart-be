using LivestreamService.Application.DTOs.Chat;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface ISignalRChatService
    {
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
    }
}