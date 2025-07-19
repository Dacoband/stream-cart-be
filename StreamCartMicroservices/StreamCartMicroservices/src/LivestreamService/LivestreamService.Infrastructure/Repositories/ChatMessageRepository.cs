using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using LivestreamService.Infrastructure.Data;
using MongoDB.Driver;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Repositories
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<ChatMessage> _collection;

        public ChatMessageRepository(MongoDbContext context)
        {
            _context = context;
            _collection = _context.ChatMessages;
        }

        // IGenericRepository implementation
        public async Task<ChatMessage?> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return null;

            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(x => x.Id, guidId),
                Builders<ChatMessage>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<ChatMessage?> FindOneAsync(Expression<Func<ChatMessage, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ChatMessage>> FilterByAsync(Expression<Func<ChatMessage, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetAllAsync()
        {
            var filter = Builders<ChatMessage>.Filter.Eq(x => x.IsDeleted, false);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task InsertAsync(ChatMessage entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task InsertManyAsync(IEnumerable<ChatMessage> entities)
        {
            await _collection.InsertManyAsync(entities);
        }

        public async Task ReplaceAsync(string id, ChatMessage entity)
        {
            if (!Guid.TryParse(id, out var guidId))
                return;

            var filter = Builders<ChatMessage>.Filter.Eq(x => x.Id, guidId);
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return;

            var filter = Builders<ChatMessage>.Filter.Eq(x => x.Id, guidId);
            var update = Builders<ChatMessage>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.LastModifiedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(filter, update);
        }

        public async Task DeleteManyAsync(IEnumerable<string> ids)
        {
            var guidIds = ids.Where(id => Guid.TryParse(id, out _))
                            .Select(id => Guid.Parse(id))
                            .ToList();

            if (!guidIds.Any())
                return;

            var filter = Builders<ChatMessage>.Filter.In(x => x.Id, guidIds);
            var update = Builders<ChatMessage>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.LastModifiedAt, DateTime.UtcNow);

            await _collection.UpdateManyAsync(filter, update);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return false;

            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(x => x.Id, guidId),
                Builders<ChatMessage>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<ChatMessage, bool>> filter)
        {
            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<int> CountAsync(Expression<Func<ChatMessage, bool>> filter)
        {
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<int> CountAsync()
        {
            var filter = Builders<ChatMessage>.Filter.Eq(x => x.IsDeleted, false);
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<PagedResult<ChatMessage>> SearchAsync(
            string searchTerm,
            PaginationParams paginationParams,
            string[]? searchableFields = null,
            Expression<Func<ChatMessage, bool>>? filter = null,
            bool exactMatch = false)
        {
            var filterBuilder = Builders<ChatMessage>.Filter;
            var baseFilter = filterBuilder.Eq(x => x.IsDeleted, false);

            // Add custom filter if provided
            if (filter != null)
            {
                baseFilter = filterBuilder.And(baseFilter, filter);
            }

            // Add search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchFilter = exactMatch
                    ? filterBuilder.Regex(x => x.Content, new MongoDB.Bson.BsonRegularExpression($"^{searchTerm}$", "i"))
                    : filterBuilder.Regex(x => x.Content, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));

                baseFilter = filterBuilder.And(baseFilter, searchFilter);
            }

            var totalCount = await _collection.CountDocumentsAsync(baseFilter);

            var messages = await _collection
                .Find(baseFilter)
                .SortByDescending(x => x.SentAt)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Limit(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResult<ChatMessage>(messages, (int)totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        // IChatMessageRepository specific methods
        public async Task<PagedResult<ChatMessage>> GetByChatRoomIdAsync(
            Guid chatRoomId,
            int pageNumber,
            int pageSize)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(x => x.ChatRoomId, chatRoomId),
                Builders<ChatMessage>.Filter.Eq(x => x.IsDeleted, false)
            );

            var totalCount = await _collection.CountDocumentsAsync(filter);

            var messages = await _collection
                .Find(filter)
                .SortByDescending(x => x.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new PagedResult<ChatMessage>(messages, (int)totalCount, pageNumber, pageSize);
        }

        // ✅ THÊM METHOD BỊ THIẾU
        public async Task<PagedResult<ChatMessage>> GetChatRoomMessagesAsync(
            Guid chatRoomId,
            int pageNumber,
            int pageSize)
        {
            // Sử dụng lại logic từ GetByChatRoomIdAsync
            return await GetByChatRoomIdAsync(chatRoomId, pageNumber, pageSize);
        }

        public async Task<ChatMessage?> GetLastMessageAsync(Guid chatRoomId)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(x => x.ChatRoomId, chatRoomId),
                Builders<ChatMessage>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection
                .Find(filter)
                .SortByDescending(x => x.SentAt)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid chatRoomId, Guid userId)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(x => x.ChatRoomId, chatRoomId),
                Builders<ChatMessage>.Filter.Ne(x => x.SenderUserId, userId),
                Builders<ChatMessage>.Filter.Eq(x => x.IsRead, false),
                Builders<ChatMessage>.Filter.Eq(x => x.IsDeleted, false)
            );

            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(Guid chatRoomId, Guid userId)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(x => x.ChatRoomId, chatRoomId),
                Builders<ChatMessage>.Filter.Ne(x => x.SenderUserId, userId),
                Builders<ChatMessage>.Filter.Eq(x => x.IsRead, false),
                Builders<ChatMessage>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection
                .Find(filter)
                .SortBy(x => x.SentAt)
                .ToListAsync();
        }

        public async Task MarkMessagesAsReadAsync(Guid chatRoomId, Guid userId)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(x => x.ChatRoomId, chatRoomId),
                Builders<ChatMessage>.Filter.Ne(x => x.SenderUserId, userId),
                Builders<ChatMessage>.Filter.Eq(x => x.IsRead, false),
                Builders<ChatMessage>.Filter.Eq(x => x.IsDeleted, false)
            );

            var update = Builders<ChatMessage>.Update
                .Set(x => x.IsRead, true)
                .Set(x => x.ReadAt, DateTime.UtcNow)
                .Set(x => x.LastModifiedAt, DateTime.UtcNow)
                .Set(x => x.LastModifiedBy, userId.ToString());

            await _collection.UpdateManyAsync(filter, update);
        }

        public async Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(Guid chatRoomId, int count)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(x => x.ChatRoomId, chatRoomId),
                Builders<ChatMessage>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection
                .Find(filter)
                .SortByDescending(x => x.SentAt)
                .Limit(count)
                .ToListAsync();
        }
    }
}