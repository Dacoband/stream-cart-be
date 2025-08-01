using System.Text.Json.Serialization;

namespace ChatBoxService.Application.DTOs
{
    /// <summary>
    /// Request model for AI chat integration
    /// </summary>
    public class AIChatRequest
    {
        /// <summary>
        /// The user's message to send to the AI
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model from AI chat service
    /// </summary>
    public class AIChatResponse
    {
        /// <summary>
        /// The AI's response message
        /// </summary>
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        /// <summary>
        /// Status of the AI response
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Additional metadata about the response
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Chat history entry from AI service
    /// </summary>
    public class AIChatHistoryEntry
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("user_message")]
        public string UserMessage { get; set; } = string.Empty;

        [JsonPropertyName("ai_response")]
        public string AIResponse { get; set; } = string.Empty;
    }

    /// <summary>
    /// Full chat history response model
    /// </summary>
    public class AIChatHistoryResponse
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("history")]
        public List<AIChatHistoryEntry> History { get; set; } = new List<AIChatHistoryEntry>();
    }
}