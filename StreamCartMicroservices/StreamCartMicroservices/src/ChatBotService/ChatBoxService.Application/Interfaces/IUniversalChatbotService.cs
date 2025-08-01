using ChatBoxService.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace ChatBoxService.Application.Interfaces
{
    /// <summary>
    /// Universal ChatBot Service cho toàn bộ platform StreamCart
    /// </summary>
    public interface IUniversalChatbotService
    {
        /// <summary>
        /// Tạo phản hồi thông minh cho customer trên toàn platform
        /// </summary>
        /// <param name="customerMessage">Tin nhắn từ customer</param>
        /// <param name="userId">ID của user (từ JWT)</param>
        /// <returns>Phản hồi AI với gợi ý shop và sản phẩm</returns>
        Task<ChatbotResponseDTO> GenerateUniversalResponseAsync(string customerMessage, Guid userId);

        /// <summary>
        /// Phân tích ý định tin nhắn universal
        /// </summary>
        /// <param name="customerMessage">Tin nhắn của customer</param>
        /// <returns>Intent analysis</returns>
        Task<ChatbotIntent> AnalyzeUniversalIntentAsync(string customerMessage);
    }
}