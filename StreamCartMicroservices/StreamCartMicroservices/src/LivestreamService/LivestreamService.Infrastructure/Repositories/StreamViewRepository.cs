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
    public class StreamViewRepository : IGenericRepository<StreamView>, IStreamViewRepository
    {
        private readonly LivestreamDbContext _context;
        private readonly ILogger<StreamViewRepository> _logger;

        public StreamViewRepository(LivestreamDbContext context, ILogger<StreamViewRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<StreamView>> GetByLivestreamIdAsync(Guid livestreamId)
        {
            try
            {
                return await _context.StreamViews
                    .Where(v => v.LivestreamId == livestreamId && !v.IsDeleted)
                    .OrderByDescending(v => v.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream views for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        public async Task<IEnumerable<StreamView>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                return await _context.StreamViews
                    .Where(v => v.UserId == userId && !v.IsDeleted)
                    .OrderByDescending(v => v.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream views for user {UserId}", userId);
                throw;
            }
        }

        public async Task<StreamView?> GetActiveViewByUserAsync(Guid livestreamId, Guid userId)
        {
            try
            {
                return await _context.StreamViews
                    .FirstOrDefaultAsync(v => v.LivestreamId == livestreamId &&
                                           v.UserId == userId &&
                                           v.EndTime == null &&
                                           !v.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active view for user {UserId} in livestream {LivestreamId}", userId, livestreamId);
                throw;
            }
        }

        public async Task<int> CountActiveViewersAsync(Guid livestreamId)
        {
            try
            {
                return await _context.StreamViews
                    .CountAsync(v => v.LivestreamId == livestreamId &&
                               v.EndTime == null &&
                               !v.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting active viewers for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        public async Task<int> CountTotalViewsAsync(Guid livestreamId)
        {
            try
            {
                return await _context.StreamViews
                    .CountAsync(v => v.LivestreamId == livestreamId && !v.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting total views for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        public async Task<int> CountUniqueViewersAsync(Guid livestreamId)
        {
            try
            {
                return await _context.StreamViews
                    .Where(v => v.LivestreamId == livestreamId && !v.IsDeleted)
                    .Select(v => v.UserId)
                    .Distinct()
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting unique viewers for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        public async Task<TimeSpan> GetAverageViewDurationAsync(Guid livestreamId)
        {
            try
            {
                var completedViews = await _context.StreamViews
                    .Where(v => v.LivestreamId == livestreamId &&
                               v.EndTime.HasValue &&
                               !v.IsDeleted)
                    .Select(v => new { v.StartTime, v.EndTime })
                    .ToListAsync();

                if (!completedViews.Any())
                    return TimeSpan.Zero;

                var totalTicks = completedViews
                    .Select(v => (v.EndTime!.Value - v.StartTime).Ticks)
                    .Sum();

                return new TimeSpan(totalTicks / completedViews.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average view duration for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        public async Task<IEnumerable<StreamView>> GetViewsWithinTimeRangeAsync(Guid livestreamId, DateTime startTime, DateTime endTime)
        {
            try
            {
                return await _context.StreamViews
                    .Where(v => v.LivestreamId == livestreamId &&
                               v.StartTime >= startTime &&
                               v.StartTime <= endTime &&
                               !v.IsDeleted)
                    .OrderBy(v => v.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting views within time range for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        // IGenericRepository implementation
        public async Task<StreamView?> GetByIdAsync(string id)
        {
            return await _context.StreamViews
                .FirstOrDefaultAsync(v => v.Id == Guid.Parse(id) && !v.IsDeleted);
        }

        public async Task<StreamView?> FindOneAsync(Expression<Func<StreamView, bool>> filter)
        {
            return await _context.StreamViews.FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<StreamView>> FilterByAsync(Expression<Func<StreamView, bool>> filter)
        {
            return await _context.StreamViews.Where(filter).ToListAsync();
        }

        public async Task<IEnumerable<StreamView>> GetAllAsync()
        {
            return await _context.StreamViews.Where(v => !v.IsDeleted).ToListAsync();
        }

        public async Task InsertAsync(StreamView entity)
        {
            await _context.StreamViews.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task InsertManyAsync(IEnumerable<StreamView> entities)
        {
            await _context.StreamViews.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task ReplaceAsync(string id, StreamView entity)
        {
            _context.StreamViews.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.StreamViews.FindAsync(Guid.Parse(id));
            if (entity != null)
            {
                entity.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteManyAsync(IEnumerable<string> ids)
        {
            var guidIds = ids.Select(Guid.Parse);
            var entities = await _context.StreamViews
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
            return await _context.StreamViews.AnyAsync(x => x.Id == Guid.Parse(id) && !x.IsDeleted);
        }

        public async Task<bool> ExistsAsync(Expression<Func<StreamView, bool>> filter)
        {
            return await _context.StreamViews.AnyAsync(filter);
        }

        public async Task<int> CountAsync(Expression<Func<StreamView, bool>> filter)
        {
            return await _context.StreamViews.CountAsync(filter);
        }

        public async Task<int> CountAsync()
        {
            return await _context.StreamViews.CountAsync(v => !v.IsDeleted);
        }

        public Task<PagedResult<StreamView>> SearchAsync(string searchTerm, PaginationParams paginationParams, string[]? searchableFields = null, Expression<Func<StreamView, bool>>? filter = null, bool exactMatch = false)
        {
            throw new NotImplementedException("Search not implemented for StreamView");
        }
    }
}