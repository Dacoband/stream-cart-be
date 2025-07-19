using LivestreamService.Domain.Entities;
using LivestreamService.Domain.Enums;
using System;

namespace LivestreamService.Application.DTOs.Chat
{
    public class LivestreamChatDTO
    {
        public Guid Id { get; set; }
        public Guid LivestreamId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public MessageType MessageType { get; set; }
        public string? ReplyToMessageId { get; set; }
        public bool IsModerated { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Additional fields for display
        public string? SenderAvatarUrl { get; set; }
        public string? ReplyToMessage { get; set; }
        public string? ReplyToSenderName { get; set; }
    }

    public class SendLivestreamMessageDTO
    {
        public Guid LivestreamId { get; set; }
        public string Message { get; set; } = string.Empty;
        public MessageType MessageType { get; set; } = MessageType.Text;
        public string? ReplyToMessageId { get; set; }
    }
}