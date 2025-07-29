using ChatBoxService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBoxService.Application.Interfaces
{
    public interface IGeminiChatbotService
    {
        /// <summary>
        /// Generates a friendly response to customer questions about products
        /// </summary>
        /// <param name="customerMessage">Customer's message</param>
        /// <param name="shopId">Shop ID for context</param>
        /// <param name="productId">Optional product ID for specific product questions</param>
        /// <returns>AI-generated friendly response</returns>
        Task<string> GenerateResponseAsync(string customerMessage, Guid shopId, Guid? productId = null);

        /// <summary>
        /// Generates product-specific responses with detailed information
        /// </summary>
        /// <param name="customerMessage">Customer's message</param>
        /// <param name="productId">Product ID</param>
        /// <returns>AI-generated response with product details</returns>
        Task<string> GenerateProductResponseAsync(string customerMessage, Guid productId);

        /// <summary>
        /// Analyzes customer message intent and determines appropriate response type
        /// </summary>
        /// <param name="customerMessage">Customer's message</param>
        /// <returns>Message intent analysis</returns>
        Task<ChatbotIntent> AnalyzeMessageIntentAsync(string customerMessage);
    }
}
