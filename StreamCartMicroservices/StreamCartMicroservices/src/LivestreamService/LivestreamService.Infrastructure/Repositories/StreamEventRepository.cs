using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using LivestreamService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Repositories
{
    public class StreamEventRepository : IGenericRepository<StreamEvent>, IStreamEventRepository
    {
        private readonly LivestreamDbContext _context;
        private readonly ILogger<StreamEventRepository> _logger;

        public StreamEventRepository(LivestreamDbContext context, ILogger<StreamEventRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<StreamEvent>> GetByLivestreamIdAsync(Guid livestreamId)
        {
            try
            {
                return await _context.StreamEvents
                    .Where(e => e.LivestreamId == livestreamId && !e.IsDeleted)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream events for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        public async Task<IEnumerable<StreamEvent>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                return await _context.StreamEvents
                    .Where(e => e.UserId == userId && !e.IsDeleted)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream events for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<StreamEvent>> GetRecentEventsByLivestreamAsync(Guid livestreamId, int count = 50)
        {
            try
            {
                return await _context.StreamEvents
                    .Where(e => e.LivestreamId == livestreamId && !e.IsDeleted)
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent stream events for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        public async Task<IEnumerable<StreamEvent>> GetEventsByTypeAsync(Guid livestreamId, string eventType)
        {
            try
            {
                return await _context.StreamEvents
                    .Where(e => e.LivestreamId == livestreamId && e.EventType == eventType && !e.IsDeleted)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream events by type {EventType} for livestream {LivestreamId}", eventType, livestreamId);
                throw;
            }
        }

        public async Task<int> CountEventsByTypeAsync(Guid livestreamId, string eventType)
        {
            try
            {
                return await _context.StreamEvents
                    .CountAsync(e => e.LivestreamId == livestreamId && e.EventType == eventType && !e.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting stream events by type {EventType} for livestream {LivestreamId}", eventType, livestreamId);
                throw;
            }
        }

        public async Task<IEnumerable<StreamEvent>> GetEventsByProductAsync(Guid livestreamProductId)
        {
            try
            {
                return await _context.StreamEvents
                    .Where(e => e.LivestreamProductId == livestreamProductId && !e.IsDeleted)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream events for product {LivestreamProductId}", livestreamProductId);
                throw;
            }
        }

        // IGenericRepository implementation
        public async Task<StreamEvent?> GetByIdAsync(string id)
        {
            return await _context.StreamEvents
                .FirstOrDefaultAsync(e => e.Id == Guid.Parse(id) && !e.IsDeleted);
        }

        public async Task<StreamEvent?> FindOneAsync(Expression<Func<StreamEvent, bool>> filter)
        {
            return await _context.StreamEvents.FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<StreamEvent>> FilterByAsync(Expression<Func<StreamEvent, bool>> filter)
        {
            return await _context.StreamEvents.Where(filter).ToListAsync();
        }

        public async Task<IEnumerable<StreamEvent>> GetAllAsync()
        {
            return await _context.StreamEvents.Where(e => !e.IsDeleted).ToListAsync();
        }

        public async Task InsertAsync(StreamEvent entity)
        {
            await _context.StreamEvents.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task InsertManyAsync(IEnumerable<StreamEvent> entities)
        {
            await _context.StreamEvents.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task ReplaceAsync(string id, StreamEvent entity)
        {
            _context.StreamEvents.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.StreamEvents.FindAsync(Guid.Parse(id));
            if (entity != null)
            {
                entity.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteManyAsync(IEnumerable<string> ids)
        {
            var guidIds = ids.Select(Guid.Parse);
            var entities = await _context.StreamEvents
                .Where(x => guidIds.Contains(x.Id))
                .ToListAsync();

            foreach (var entity in entities)
            {
                entity.Delete();
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.StreamEvents.AnyAsync(x => x.Id == Guid.Parse(id) && !x.IsDeleted);
        }

        public async Task<bool> ExistsAsync(Expression<Func<StreamEvent, bool>> filter)
        {
            return await _context.StreamEvents.AnyAsync(filter);
        }

        public async Task<int> CountAsync(Expression<Func<StreamEvent, bool>> filter)
        {
            return await _context.StreamEvents.CountAsync(filter);
        }

        public async Task<int> CountAsync()
        {
            return await _context.StreamEvents.CountAsync(e => !e.IsDeleted);
        }

        public Task<PagedResult<StreamEvent>> SearchAsync(string searchTerm, PaginationParams paginationParams, string[]? searchableFields = null, Expression<Func<StreamEvent, bool>>? filter = null, bool exactMatch = false)
        {
            throw new NotImplementedException("Search not implemented for StreamEvent");
        }
    }
}