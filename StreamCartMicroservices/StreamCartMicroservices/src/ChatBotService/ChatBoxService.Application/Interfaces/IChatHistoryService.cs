using ChatBoxService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBoxService.Application.Interfaces
{
    /// <summary>
    /// Service để quản lý lịch sử chat với Redis
    /// </summary>
    public interface IChatHistoryService
    {
        /// <summary>
        /// Lưu cuộc trò chuyện vào Redis
        /// </summary>
        /// <param name="conversation">Cuộc trò chuyện cần lưu</param>
        /// <param name="expireTimeMinutes">Thời gian hết hạn (phút), mặc định 1440 phút (24 giờ)</param>
        Task SaveConversationAsync(ConversationHistory conversation, int expireTimeMinutes = 1440);

        /// <summary>
        /// Lấy cuộc trò chuyện theo ID
        /// </summary>
        /// <param name="conversationId">ID cuộc trò chuyện</param>
        Task<ConversationHistory?> GetConversationAsync(string conversationId);

        /// <summary>
        /// Lấy hoặc tạo cuộc trò chuyện cho user và shop
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="shopId">ID shop</param>
        /// <param name="sessionId">Session ID (optional)</param>
        Task<ConversationHistory> GetOrCreateConversationAsync(Guid userId, Guid shopId, string? sessionId = null);

        /// <summary>
        /// Thêm tin nhắn vào cuộc trò chuyện
        /// </summary>
        /// <param name="conversationId">ID cuộc trò chuyện</param>
        /// <param name="content">Nội dung tin nhắn</param>
        /// <param name="sender">Người gửi (User/Bot)</param>
        /// <param name="intent">Intent của tin nhắn</param>
        /// <param name="confidence">Độ tin cậy</param>
        Task AddMessageToConversationAsync(string conversationId, string content, string sender, string intent = "", decimal confidence = 0);

        /// <summary>
        /// Lấy context từ cuộc trò chuyện gần nhất
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="shopId">ID shop</param>
        /// <param name="messageCount">Số tin nhắn gần nhất</param>
        Task<string> GetConversationContextAsync(Guid userId, Guid shopId, int messageCount = 5);

        /// <summary>
        /// Lấy lịch sử chat của user với shop
        /// </summary>
        /// <param name="request">Request chứa thông tin filter</param>
        Task<ChatHistoryResponse> GetChatHistoryAsync(GetChatHistoryRequest request);

        /// <summary>
        /// Xóa cuộc trò chuyện
        /// </summary>
        /// <param name="conversationId">ID cuộc trò chuyện</param>
        Task DeleteConversationAsync(string conversationId);

        /// <summary>
        /// Xóa tất cả cuộc trò chuyện của user với shop
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="shopId">ID shop</param>
        Task DeleteUserConversationsAsync(Guid userId, Guid shopId);

        /// <summary>
        /// Gia hạn thời gian sống của cuộc trò chuyện
        /// </summary>
        /// <param name="conversationId">ID cuộc trò chuyện</param>
        /// <param name="expireTimeMinutes">Thời gian gia hạn (phút)</param>
        Task ExtendConversationExpiryAsync(string conversationId, int expireTimeMinutes = 1440);
    }
}