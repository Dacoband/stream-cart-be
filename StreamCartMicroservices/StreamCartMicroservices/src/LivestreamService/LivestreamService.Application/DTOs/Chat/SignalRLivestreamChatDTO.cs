using System;
using LivestreamService.Domain.Enums;

namespace LivestreamService.Application.DTOs.Chat
{
    /// <summary>
    /// DTO đặc biệt cho tin nhắn Livestream qua SignalR, không chứa trường LiveKit
    /// </summary>
    public class SignalRLivestreamChatDTO
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
        public string? SenderAvatarUrl { get; set; }
        public string? ReplyToMessage { get; set; }
        public string? ReplyToSenderName { get; set; }

        // Phương thức chuyển đổi từ LivestreamChatDTO sang SignalRLivestreamChatDTO
        public static SignalRLivestreamChatDTO FromLivestreamChatDTO(LivestreamChatDTO source)
        {
            return new SignalRLivestreamChatDTO
            {
                Id = source.Id,
                LivestreamId = source.LivestreamId,
                SenderId = source.SenderId,
                SenderName = source.SenderName,
                SenderType = source.SenderType,
                Message = source.Message,
                MessageType = source.MessageType,
                ReplyToMessageId = source.ReplyToMessageId,
                IsModerated = source.IsModerated,
                SentAt = source.SentAt,
                CreatedAt = source.CreatedAt,
                SenderAvatarUrl = source.SenderAvatarUrl,
                ReplyToMessage = source.ReplyToMessage,
                ReplyToSenderName = source.ReplyToSenderName
            };
        }
    }
}