using ChatBoxService.Application.DTOs;
using ChatBoxService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatBoxService.Infrastructure.Services
{
    public class AIChatService : IAIChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AIChatService> _logger;
        private readonly string _aiServiceUrl;

        public AIChatService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AIChatService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _aiServiceUrl = configuration["AI_SERVICE_URL"] ?? "https://ai.dacoban.studio";
        }

        public async Task<AIChatResponse> SendMessageAsync(string message, string userId)
        {
            try
            {
                _logger.LogInformation("Sending message to AI service for user {UserId}", userId);

                using var client = _httpClientFactory.CreateClient();
                var request = new
                {
                    message = message,
                    user_id = userId
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync($"{_aiServiceUrl}/chat", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AIChatResponse>();
                    return result ?? new AIChatResponse
                    {
                        Response = "Sorry, I couldn't process your request at this time.",
                        Status = "error"
                    };
                }

                _logger.LogWarning("AI service returned non-success status code: {StatusCode}", response.StatusCode);
                return new AIChatResponse
                {
                    Response = "There was an issue connecting to the AI service. Please try again later.",
                    Status = "error"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to AI service");
                return new AIChatResponse
                {
                    Response = "Sorry, I encountered an error processing your request.",
                    Status = "error"
                };
            }
        }

        public async Task<AIChatHistoryResponse> GetChatHistoryAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting chat history from AI service for user {UserId}", userId);

                using var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{_aiServiceUrl}/user/{userId}/history");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AIChatHistoryResponse>();
                    return result ?? new AIChatHistoryResponse
                    {
                        UserId = userId,
                        History = new List<AIChatHistoryEntry>()
                    };
                }

                _logger.LogWarning("AI service returned non-success status code: {StatusCode}", response.StatusCode);
                return new AIChatHistoryResponse { UserId = userId, History = new List<AIChatHistoryEntry>() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat history from AI service");
                return new AIChatHistoryResponse { UserId = userId, History = new List<AIChatHistoryEntry>() };
            }
        }
    }
}