using ChatBoxService.Application.DTOs;
using ChatBoxService.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatBoxService.Infrastructure.Services
{
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly StackExchange.Redis.IDatabase _database;
        private readonly ILogger<ChatHistoryService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        // Redis key patterns
        private const string CONVERSATION_KEY_PREFIX = "chatbot:conversation";
        private const string USER_CONVERSATIONS_KEY_PREFIX = "chatbot:user_conversations";
        private const string SHOP_CONVERSATIONS_KEY_PREFIX = "chatbot:shop_conversations";

        public ChatHistoryService(IConnectionMultiplexer redis, ILogger<ChatHistoryService> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task SaveConversationAsync(ConversationHistory conversation, int expireTimeMinutes = 1440)
        {
            try
            {
                var conversationKey = GetConversationKey(conversation.ConversationId);
                var userConversationsKey = GetUserConversationsKey(conversation.UserId);
                var shopConversationsKey = GetShopConversationsKey(conversation.ShopId);

                conversation.LastUpdated = DateTime.UtcNow;
                var jsonData = JsonSerializer.Serialize(conversation, _jsonOptions);

                var expiry = TimeSpan.FromMinutes(expireTimeMinutes);

                // Lưu conversation data
                await _database.StringSetAsync(conversationKey, jsonData, expiry);

                // Thêm vào danh sách conversations của user
                await _database.SortedSetAddAsync(userConversationsKey, conversation.ConversationId,
                    conversation.LastUpdated.Ticks);
                await _database.KeyExpireAsync(userConversationsKey, expiry);

                // Thêm vào danh sách conversations của shop
                await _database.SortedSetAddAsync(shopConversationsKey, conversation.ConversationId,
                    conversation.LastUpdated.Ticks);
                await _database.KeyExpireAsync(shopConversationsKey, expiry);

                _logger.LogInformation("Saved conversation {ConversationId} for user {UserId} and shop {ShopId}",
                    conversation.ConversationId, conversation.UserId, conversation.ShopId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving conversation {ConversationId}", conversation.ConversationId);
                throw;
            }
        }

        public async Task<ConversationHistory?> GetConversationAsync(string conversationId)
        {
            try
            {
                var conversationKey = GetConversationKey(conversationId);
                var jsonData = await _database.StringGetAsync(conversationKey);

                if (!jsonData.HasValue)
                {
                    _logger.LogWarning("Conversation {ConversationId} not found in Redis", conversationId);
                    return null;
                }

                var conversation = JsonSerializer.Deserialize<ConversationHistory>(jsonData!, _jsonOptions);
                return conversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation {ConversationId}", conversationId);
                return null;
            }
        }

        public async Task<ConversationHistory> GetOrCreateConversationAsync(Guid userId, Guid shopId, string? sessionId = null)
        {
            try
            {
                // Tạo conversation ID dựa trên userId, shopId và session
                var conversationId = GenerateConversationId(userId, shopId, sessionId);

                // Thử lấy conversation hiện có
                var existingConversation = await GetConversationAsync(conversationId);
                if (existingConversation != null)
                {
                    // Gia hạn thời gian sống
                    await ExtendConversationExpiryAsync(conversationId);
                    return existingConversation;
                }

                // Tạo conversation mới
                var newConversation = new ConversationHistory
                {
                    ConversationId = conversationId,
                    UserId = userId,
                    ShopId = shopId,
                    SessionId = sessionId ?? Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                    Messages = new List<ChatMessage>()
                };

                await SaveConversationAsync(newConversation);

                _logger.LogInformation("Created new conversation {ConversationId} for user {UserId} and shop {ShopId}",
                    conversationId, userId, shopId);

                return newConversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating conversation for user {UserId} and shop {ShopId}",
                    userId, shopId);
                throw;
            }
        }

        public async Task AddMessageToConversationAsync(string conversationId, string content, string sender,
            string intent = "", decimal confidence = 0)
        {
            try
            {
                var conversation = await GetConversationAsync(conversationId);
                if (conversation == null)
                {
                    _logger.LogWarning("Cannot add message to non-existent conversation {ConversationId}", conversationId);
                    return;
                }

                conversation.AddMessage(content, sender, intent, confidence);

                // Giới hạn số lượng tin nhắn để tránh Redis key quá lớn
                if (conversation.Messages.Count > 100)
                {
                    conversation.Messages = conversation.Messages
                        .OrderByDescending(m => m.Timestamp)
                        .Take(50)
                        .OrderBy(m => m.Timestamp)
                        .ToList();
                }

                await SaveConversationAsync(conversation);

                _logger.LogInformation("Added {Sender} message to conversation {ConversationId}",
                    sender, conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding message to conversation {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<string> GetConversationContextAsync(Guid userId, Guid shopId, int messageCount = 5)
        {
            try
            {
                var conversationId = GenerateConversationId(userId, shopId);
                var conversation = await GetConversationAsync(conversationId);

                if (conversation == null || !conversation.Messages.Any())
                {
                    return string.Empty;
                }

                return conversation.GetContextFromRecentMessages(messageCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation context for user {UserId} and shop {ShopId}",
                    userId, shopId);
                return string.Empty;
            }
        }

        public async Task<ChatHistoryResponse> GetChatHistoryAsync(GetChatHistoryRequest request)
        {
            try
            {
                var userConversationsKey = GetUserConversationsKey(request.UserId);

                // Lấy danh sách conversation IDs của user (sorted by last updated)
                var conversationIds = await _database.SortedSetRangeByScoreAsync(
                    userConversationsKey,
                    order: Order.Descending,
                    skip: (request.PageNumber - 1) * request.PageSize,
                    take: request.PageSize);

                var conversations = new List<ConversationHistory>();

                foreach (var conversationId in conversationIds)
                {
                    var conversation = await GetConversationAsync(conversationId!);
                    if (conversation != null && conversation.ShopId == request.ShopId)
                    {
                        conversations.Add(conversation);
                    }
                }

                var totalCount = await _database.SortedSetLengthAsync(userConversationsKey);

                return new ChatHistoryResponse
                {
                    Conversations = conversations,
                    TotalConversations = (int)totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat history for user {UserId}", request.UserId);
                return new ChatHistoryResponse();
            }
        }

        public async Task DeleteConversationAsync(string conversationId)
        {
            try
            {
                var conversation = await GetConversationAsync(conversationId);
                if (conversation == null) return;

                var conversationKey = GetConversationKey(conversationId);
                var userConversationsKey = GetUserConversationsKey(conversation.UserId);
                var shopConversationsKey = GetShopConversationsKey(conversation.ShopId);

                // Xóa conversation data
                await _database.KeyDeleteAsync(conversationKey);

                // Xóa khỏi danh sách của user và shop
                await _database.SortedSetRemoveAsync(userConversationsKey, conversationId);
                await _database.SortedSetRemoveAsync(shopConversationsKey, conversationId);

                _logger.LogInformation("Deleted conversation {ConversationId}", conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task DeleteUserConversationsAsync(Guid userId, Guid shopId)
        {
            try
            {
                var userConversationsKey = GetUserConversationsKey(userId);
                var conversationIds = await _database.SortedSetRangeByScoreAsync(userConversationsKey);

                foreach (var conversationId in conversationIds)
                {
                    var conversation = await GetConversationAsync(conversationId!);
                    if (conversation != null && conversation.ShopId == shopId)
                    {
                        await DeleteConversationAsync(conversationId!);
                    }
                }

                _logger.LogInformation("Deleted all conversations for user {UserId} and shop {ShopId}", userId, shopId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user conversations for user {UserId} and shop {ShopId}",
                    userId, shopId);
                throw;
            }
        }

        public async Task ExtendConversationExpiryAsync(string conversationId, int expireTimeMinutes = 1440)
        {
            try
            {
                var conversationKey = GetConversationKey(conversationId);
                var expiry = TimeSpan.FromMinutes(expireTimeMinutes);

                await _database.KeyExpireAsync(conversationKey, expiry);

                _logger.LogDebug("Extended expiry for conversation {ConversationId} by {Minutes} minutes",
                    conversationId, expireTimeMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending conversation expiry {ConversationId}", conversationId);
                throw;
            }
        }

        #region Private Helper Methods

        private static string GetConversationKey(string conversationId)
            => $"{CONVERSATION_KEY_PREFIX}:{conversationId}";

        private static string GetUserConversationsKey(Guid userId)
            => $"{USER_CONVERSATIONS_KEY_PREFIX}:{userId}";

        private static string GetShopConversationsKey(Guid shopId)
            => $"{SHOP_CONVERSATIONS_KEY_PREFIX}:{shopId}";

        private static string GenerateConversationId(Guid userId, Guid shopId, string? sessionId = null)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return $"user_{userId}_shop_{shopId}";
            }
            return $"user_{userId}_shop_{shopId}_session_{sessionId}";
        }

        #endregion
    }
}