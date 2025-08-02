using LivestreamService.Application.DTOs.Chat;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface IChatNotificationServiceSignalR
    {
        /// <summary>
        /// Notifica a un usuario sobre un nuevo mensaje de chat
        /// </summary>
        Task NotifyNewChatMessageAsync(Guid userId, ChatMessageDTO message);

        /// <summary>
        /// Notifica a un usuario sobre un nuevo mensaje de chat en livestream
        /// </summary>
        Task NotifyNewLivestreamMessageAsync(Guid userId, LivestreamChatDTO message);

        /// <summary>
        /// Notifica a un usuario que tiene mensajes sin leer
        /// </summary>
        Task NotifyUnreadMessagesAsync(Guid userId, int unreadCount, Guid chatRoomId, string senderName);

        /// <summary>
        /// Notifica a un usuario que ha sido mencionado en un chat
        /// </summary>
        Task NotifyMentionAsync(Guid userId, ChatMessageDTO message, string mentionContext);

        /// <summary>
        /// Notifica sobre la creación de una nueva sala de chat
        /// </summary>
        Task NotifyNewChatRoomAsync(Guid recipientId, Guid chatRoomId, object chatRoomInfo);

        /// <summary>
        /// Notifica a los usuarios en una sala de chat que los mensajes han sido leídos
        /// </summary>
        Task NotifyMessagesReadAsync(Guid recipientId, Guid chatRoomId, Guid readByUserId);

        /// <summary>
        /// Notifica sobre el estado de escritura de un usuario
        /// </summary>
        Task NotifyTypingStatusAsync(Guid chatRoomId, Guid userId, string userName, bool isTyping);

        /// <summary>
        /// Envía un mensaje a una sala de chat
        /// </summary>
        Task SendMessageToChatRoomAsync(Guid chatRoomId, Guid senderId, string senderName, string message);

        /// <summary>
        /// Notifica que un usuario se ha unido a una sala de chat
        /// </summary>
        Task NotifyUserJoinedChatRoomAsync(Guid chatRoomId, Guid userId, string userName);

        /// <summary>
        /// Envía un mensaje a un livestream
        /// </summary>
        Task SendMessageToLivestreamAsync(Guid livestreamId, Guid senderId, string senderName, string message);
        Task NotifyMessageEditAsync<T>(Guid chatRoomId, T message);

        /// <summary>
        /// Notifica sobre la moderación de un mensaje
        /// </summary>
        Task NotifyMessageModerationAsync(Guid livestreamId, Guid messageId, bool isModerated);
    }
}