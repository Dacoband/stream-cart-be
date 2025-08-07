using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using LivestreamService.Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Repositories
{
    public class ChatRoomRepository : IChatRoomRepository
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<ChatRoom> _collection;

        public ChatRoomRepository(MongoDbContext context)
        {
            _context = context;
            _collection = _context.ChatRooms;
        }

        // IGenericRepository implementation
        public async Task<ChatRoom?> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return null;

            var filter = Builders<ChatRoom>.Filter.And(
                Builders<ChatRoom>.Filter.Eq(x => x.Id, guidId),
                Builders<ChatRoom>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<ChatRoom?> FindOneAsync(Expression<Func<ChatRoom, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ChatRoom>> FilterByAsync(Expression<Func<ChatRoom, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<ChatRoom>> GetAllAsync()
        {
            var filter = Builders<ChatRoom>.Filter.Eq(x => x.IsDeleted, false);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task InsertAsync(ChatRoom entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task InsertManyAsync(IEnumerable<ChatRoom> entities)
        {
            await _collection.InsertManyAsync(entities);
        }

        public async Task ReplaceAsync(string id, ChatRoom entity)
        {
            if (!Guid.TryParse(id, out var guidId))
                return;

            var filter = Builders<ChatRoom>.Filter.Eq(x => x.Id, guidId);
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return;

            var filter = Builders<ChatRoom>.Filter.Eq(x => x.Id, guidId);
            var update = Builders<ChatRoom>.Update
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

            var filter = Builders<ChatRoom>.Filter.In(x => x.Id, guidIds);
            var update = Builders<ChatRoom>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.LastModifiedAt, DateTime.UtcNow);

            await _collection.UpdateManyAsync(filter, update);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return false;

            var filter = Builders<ChatRoom>.Filter.And(
                Builders<ChatRoom>.Filter.Eq(x => x.Id, guidId),
                Builders<ChatRoom>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<ChatRoom, bool>> filter)
        {
            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<int> CountAsync(Expression<Func<ChatRoom, bool>> filter)
        {
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<int> CountAsync()
        {
            var filter = Builders<ChatRoom>.Filter.Eq(x => x.IsDeleted, false);
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<PagedResult<ChatRoom>> SearchAsync(
     string searchTerm,
     PaginationParams paginationParams,
     string[]? searchableFields = null,
     Expression<Func<ChatRoom, bool>>? filter = null,
     bool exactMatch = false)
        {
            var filterBuilder = Builders<ChatRoom>.Filter;
            var baseFilter = filterBuilder.Eq(x => x.IsDeleted, false);

            // Add custom filter if provided
            if (filter != null)
            {
                baseFilter = filterBuilder.And(baseFilter, filter);
            }

            // Add search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (Guid.TryParse(searchTerm, out var searchGuid))
                {
                    var guidFilter = filterBuilder.Or(
                        filterBuilder.Eq(x => x.UserId, searchGuid),
                        filterBuilder.Eq(x => x.ShopId, searchGuid),
                        filterBuilder.Eq(x => x.RelatedOrderId, searchGuid)
                    );
                    baseFilter = filterBuilder.And(baseFilter, guidFilter);
                }
            }

            var totalCount = await _collection.CountDocumentsAsync(baseFilter);

            // ✅ Sửa thành multiple fields sort
            var sort = Builders<ChatRoom>.Sort
                .Descending(x => x.LastMessageAt)
                .Descending(x => x.StartedAt);

            var chatRooms = await _collection
                .Find(baseFilter)
                .Sort(sort)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Limit(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResult<ChatRoom>(chatRooms, (int)totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        // IChatRoomRepository specific methods
        public async Task<ChatRoom?> GetByUserAndShopAsync(Guid userId, Guid shopId)
        {
            var filter = Builders<ChatRoom>.Filter.And(
                Builders<ChatRoom>.Filter.Eq(x => x.UserId, userId),
                Builders<ChatRoom>.Filter.Eq(x => x.ShopId, shopId),
                Builders<ChatRoom>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<PagedResult<ChatRoom>> GetUserChatRoomsAsync(
    Guid userId,
    int pageNumber,
    int pageSize,
    bool? isActive = null)
        {
            var filterBuilder = Builders<ChatRoom>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(x => x.UserId, userId),
                filterBuilder.Eq(x => x.IsDeleted, false)
            );

            if (isActive.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.IsActive, isActive.Value));
            }

            var totalCount = await _collection.CountDocumentsAsync(filter);

            // ✅ Sử dụng Sort với multiple fields thay vì aggregation
            var sort = Builders<ChatRoom>.Sort
                .Descending(x => x.LastMessageAt)
                .Descending(x => x.StartedAt);

            var chatRooms = await _collection
                .Find(filter)
                .Sort(sort)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new PagedResult<ChatRoom>(chatRooms, (int)totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<ChatRoom>> GetShopChatRoomsAsync(
    Guid shopId,
    int pageNumber,
    int pageSize,
    bool? isActive = null)
        {
            var filterBuilder = Builders<ChatRoom>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(x => x.ShopId, shopId),
                filterBuilder.Eq(x => x.IsDeleted, false)
            );

            if (isActive.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.IsActive, isActive.Value));
            }

            var totalCount = await _collection.CountDocumentsAsync(filter);

            // ✅ Sửa thành multiple fields sort
            var sort = Builders<ChatRoom>.Sort
                .Descending(x => x.LastMessageAt)
                .Descending(x => x.StartedAt);

            var chatRooms = await _collection
                .Find(filter)
                .Sort(sort)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new PagedResult<ChatRoom>(chatRooms, (int)totalCount, pageNumber, pageSize);
        }

        public async Task<IEnumerable<ChatRoom>> GetActiveChatRoomsAsync(Guid userId)
        {
            var filter = Builders<ChatRoom>.Filter.And(
                Builders<ChatRoom>.Filter.Eq(x => x.UserId, userId),
                Builders<ChatRoom>.Filter.Eq(x => x.IsActive, true),
                Builders<ChatRoom>.Filter.Eq(x => x.IsDeleted, false)
            );

            // ✅ Sửa thành multiple fields sort
            var sort = Builders<ChatRoom>.Sort
                .Descending(x => x.LastMessageAt)
                .Descending(x => x.StartedAt);

            return await _collection
                .Find(filter)
                .Sort(sort)
                .ToListAsync();
        }
        public async Task<IEnumerable<ChatRoom>> GetChatRoomsByUserIdAsync(Guid userId)
        {
            var filter = Builders<ChatRoom>.Filter.And(
                Builders<ChatRoom>.Filter.Eq(x => x.UserId, userId),
                Builders<ChatRoom>.Filter.Eq(x => x.IsDeleted, false)
            );

            // Use the same sort order as other methods
            var sort = Builders<ChatRoom>.Sort
                .Descending(x => x.LastMessageAt)
                .Descending(x => x.StartedAt);

            return await _collection
                .Find(filter)
                .Sort(sort)
                .ToListAsync();
        }
    }
}