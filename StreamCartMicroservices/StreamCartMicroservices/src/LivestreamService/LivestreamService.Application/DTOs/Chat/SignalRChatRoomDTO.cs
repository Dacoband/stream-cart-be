using System;

namespace LivestreamService.Application.DTOs.Chat.SignalR
{
    /// <summary>
    /// DTO chuyên biệt cho chat room qua SignalR, không chứa trường LiveKit
    /// </summary>
    public class SignalRChatRoomDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ShopId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public bool IsActive { get; set; }
        public int UnreadCount { get; set; }
        // Không chứa LiveKitRoomName hoặc các thông tin LiveKit khác
    }
}