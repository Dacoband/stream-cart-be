using System;
using System.Collections.Generic;

namespace LivestreamService.Application.DTOs.Chat
{
    public class ChatRoomDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ShopId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public Guid? RelatedOrderId { get; set; }
        public bool IsActive { get; set; }

        // Additional info
        public string? UserName { get; set; }
        public string? UserAvatarUrl { get; set; }
        public string? ShopName { get; set; }
        public string? ShopLogoUrl { get; set; }
        public ChatMessageDTO? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        //public string? LiveKitRoomName { get; set; }
        //public bool IsLiveKitActive { get; set; }
    }

    public class ChatMessageDTO
    {
        public Guid Id { get; set; }
        public Guid ChatRoomId { get; set; }
        public Guid SenderUserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsEdited { get; set; }
        public string MessageType { get; set; } = "Text";
        public string? AttachmentUrl { get; set; }
        public DateTime? EditedAt { get; set; }

        // Additional info
        public string? SenderName { get; set; }
        public string? SenderAvatarUrl { get; set; }
        public bool IsMine { get; set; }
    }

    public class SendChatMessageDTO
    {
        public Guid ChatRoomId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text";
        public string? AttachmentUrl { get; set; }
    }

    public class CreateChatRoomDTO
    {
        public Guid ShopId { get; set; }
        public Guid? RelatedOrderId { get; set; }
        public string? InitialMessage { get; set; }
    }

    public class EditChatMessageDTO
    {
        public string Content { get; set; } = string.Empty;
    }
}