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
    public class LivestreamChatRepository : ILivestreamChatRepository
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<LivestreamChat> _collection;

        public LivestreamChatRepository(MongoDbContext context)
        {
            _context = context;
            _collection = _context.LivestreamChats;
        }

        // IGenericRepository implementation
        public async Task<LivestreamChat?> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return null;

            var filter = Builders<LivestreamChat>.Filter.And(
                Builders<LivestreamChat>.Filter.Eq(x => x.Id, guidId),
                Builders<LivestreamChat>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<LivestreamChat?> FindOneAsync(Expression<Func<LivestreamChat, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LivestreamChat>> FilterByAsync(Expression<Func<LivestreamChat, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<LivestreamChat>> GetAllAsync()
        {
            var filter = Builders<LivestreamChat>.Filter.Eq(x => x.IsDeleted, false);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task InsertAsync(LivestreamChat entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task InsertManyAsync(IEnumerable<LivestreamChat> entities)
        {
            await _collection.InsertManyAsync(entities);
        }

        public async Task ReplaceAsync(string id, LivestreamChat entity)
        {
            if (!Guid.TryParse(id, out var guidId))
                return;

            var filter = Builders<LivestreamChat>.Filter.Eq(x => x.Id, guidId);
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return;

            var filter = Builders<LivestreamChat>.Filter.Eq(x => x.Id, guidId);
            var update = Builders<LivestreamChat>.Update
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

            var filter = Builders<LivestreamChat>.Filter.In(x => x.Id, guidIds);
            var update = Builders<LivestreamChat>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.LastModifiedAt, DateTime.UtcNow);

            await _collection.UpdateManyAsync(filter, update);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return false;

            var filter = Builders<LivestreamChat>.Filter.And(
                Builders<LivestreamChat>.Filter.Eq(x => x.Id, guidId),
                Builders<LivestreamChat>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<LivestreamChat, bool>> filter)
        {
            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<int> CountAsync(Expression<Func<LivestreamChat, bool>> filter)
        {
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<int> CountAsync()
        {
            var filter = Builders<LivestreamChat>.Filter.Eq(x => x.IsDeleted, false);
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<PagedResult<LivestreamChat>> SearchAsync(
            string searchTerm,
            PaginationParams paginationParams,
            string[]? searchableFields = null,
            Expression<Func<LivestreamChat, bool>>? filter = null,
            bool exactMatch = false)
        {
            var filterBuilder = Builders<LivestreamChat>.Filter;
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
                    ? filterBuilder.Regex(x => x.Message, new MongoDB.Bson.BsonRegularExpression($"^{searchTerm}$", "i"))
                    : filterBuilder.Regex(x => x.Message, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));

                if (searchableFields != null && searchableFields.Length > 0)
                {
                    var fieldFilters = new List<FilterDefinition<LivestreamChat>>();
                    foreach (var field in searchableFields)
                    {
                        switch (field.ToLower())
                        {
                            case "message":
                                fieldFilters.Add(exactMatch
                                    ? filterBuilder.Regex(x => x.Message, new MongoDB.Bson.BsonRegularExpression($"^{searchTerm}$", "i"))
                                    : filterBuilder.Regex(x => x.Message, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")));
                                break;
                            case "sendername":
                                fieldFilters.Add(exactMatch
                                    ? filterBuilder.Regex(x => x.SenderName, new MongoDB.Bson.BsonRegularExpression($"^{searchTerm}$", "i"))
                                    : filterBuilder.Regex(x => x.SenderName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")));
                                break;
                        }
                    }
                    if (fieldFilters.Any())
                    {
                        searchFilter = filterBuilder.Or(fieldFilters);
                    }
                }

                baseFilter = filterBuilder.And(baseFilter, searchFilter);
            }

            var totalCount = await _collection.CountDocumentsAsync(baseFilter);

            var messages = await _collection
                .Find(baseFilter)
                .SortByDescending(x => x.SentAt)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Limit(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResult<LivestreamChat>(messages, (int)totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        // ILivestreamChatRepository specific methods
        public async Task<PagedResult<LivestreamChat>> GetLivestreamChatAsync(
            Guid livestreamId,
            int pageNumber,
            int pageSize,
            bool includeModerated = false)
        {
            var filterBuilder = Builders<LivestreamChat>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(x => x.LivestreamId, livestreamId),
                filterBuilder.Eq(x => x.IsDeleted, false)
            );

            if (!includeModerated)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.IsModerated, false));
            }

            var totalCount = await _collection.CountDocumentsAsync(filter);

            var messages = await _collection
                .Find(filter)
                .SortByDescending(x => x.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new PagedResult<LivestreamChat>(messages, (int)totalCount, pageNumber, pageSize);
        }

        public async Task<IEnumerable<LivestreamChat>> GetByLivestreamIdAsync(Guid livestreamId)
        {
            var filter = Builders<LivestreamChat>.Filter.And(
                Builders<LivestreamChat>.Filter.Eq(x => x.LivestreamId, livestreamId),
                Builders<LivestreamChat>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection
                .Find(filter)
                .SortByDescending(x => x.SentAt)
                .ToListAsync();
        }

        public async Task<LivestreamChat?> GetMessageByIdAsync(Guid messageId)
        {
            var filter = Builders<LivestreamChat>.Filter.And(
                Builders<LivestreamChat>.Filter.Eq(x => x.Id, messageId),
                Builders<LivestreamChat>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LivestreamChat>> GetRecentMessagesAsync(Guid livestreamId, int limit = 50)
        {
            var filter = Builders<LivestreamChat>.Filter.And(
                Builders<LivestreamChat>.Filter.Eq(x => x.LivestreamId, livestreamId),
                Builders<LivestreamChat>.Filter.Eq(x => x.IsDeleted, false),
                Builders<LivestreamChat>.Filter.Eq(x => x.IsModerated, false)
            );

            return await _collection
                .Find(filter)
                .SortByDescending(x => x.SentAt)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnmoderatedMessageCountAsync(Guid livestreamId)
        {
            var filter = Builders<LivestreamChat>.Filter.And(
                Builders<LivestreamChat>.Filter.Eq(x => x.LivestreamId, livestreamId),
                Builders<LivestreamChat>.Filter.Eq(x => x.IsDeleted, false),
                Builders<LivestreamChat>.Filter.Eq(x => x.IsModerated, false)
            );

            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<IEnumerable<LivestreamChat>> GetMessagesByUserAsync(Guid livestreamId, Guid userId)
        {
            var filter = Builders<LivestreamChat>.Filter.And(
                Builders<LivestreamChat>.Filter.Eq(x => x.LivestreamId, livestreamId),
                Builders<LivestreamChat>.Filter.Eq(x => x.SenderId, userId),
                Builders<LivestreamChat>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await _collection
                .Find(filter)
                .SortByDescending(x => x.SentAt)
                .ToListAsync();
        }
    }
}