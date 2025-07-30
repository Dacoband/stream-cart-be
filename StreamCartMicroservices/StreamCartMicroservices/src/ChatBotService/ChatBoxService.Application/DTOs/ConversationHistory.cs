using System;
using System.Collections.Generic;

namespace ChatBoxService.Application.DTOs
{
    /// <summary>
    /// DTO cho lịch sử cuộc trò chuyện
    /// </summary>
    public class ConversationHistory
    {
        public string ConversationId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public Guid ShopId { get; set; }
        public List<ChatMessage> Messages { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Thêm tin nhắn mới vào lịch sử
        /// </summary>
        public void AddMessage(string content, string sender, string intent = "", decimal confidence = 0)
        {
            Messages.Add(new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                Content = content,
                Sender = sender,
                Intent = intent,
                Confidence = confidence,
                Timestamp = DateTime.UtcNow
            });
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Lấy context từ N tin nhắn gần nhất
        /// </summary>
        public string GetContextFromRecentMessages(int messageCount = 5)
        {
            var recentMessages = Messages
                .OrderByDescending(m => m.Timestamp)
                .Take(messageCount)
                .OrderBy(m => m.Timestamp)
                .ToList();

            if (!recentMessages.Any())
                return string.Empty;

            var context = "LỊCH SỬ CUỘC TRÒ CHUYỆN GẦN ĐÂY:\n";
            foreach (var msg in recentMessages)
            {
                context += $"{msg.Sender}: {msg.Content}\n";
            }
            context += "---\n";

            return context;
        }
    }

    /// <summary>
    /// DTO cho một tin nhắn trong cuộc trò chuyện
    /// </summary>
    public class ChatMessage
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty; // "User" hoặc "Bot"
        public string Intent { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Request để lấy lịch sử chat
    /// </summary>
    public class GetChatHistoryRequest
    {
        public Guid UserId { get; set; }
        public Guid ShopId { get; set; }
        public string? SessionId { get; set; }
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
    }

    /// <summary>
    /// Response cho lịch sử chat
    /// </summary>
    public class ChatHistoryResponse
    {
        public List<ConversationHistory> Conversations { get; set; } = new();
        public int TotalConversations { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}