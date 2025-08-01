using ChatBoxService.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace ChatBoxService.Application.Interfaces
{
    /// <summary>
    /// Service for external AI chat integration
    /// </summary>
    public interface IAIChatService
    {
        /// <summary>
        /// Send a message to the external AI chat service
        /// </summary>
        /// <param name="message">User's message</param>
        /// <param name="userId">User's ID from JWT</param>
        /// <returns>AI chat response</returns>
        Task<AIChatResponse> SendMessageAsync(string message, string userId);

        /// <summary>
        /// Get chat history for a specific user
        /// </summary>
        /// <param name="userId">User's ID from JWT</param>
        /// <returns>Chat history for the user</returns>
        Task<AIChatHistoryResponse> GetChatHistoryAsync(string userId);
    }
}