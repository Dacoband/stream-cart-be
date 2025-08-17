using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using LivestreamService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Domain.Bases;
using System.Linq.Expressions;

namespace LivestreamService.Infrastructure.Repositories
{
    public class LivestreamCartRepository : ILivestreamCartRepository
    {
        private readonly LivestreamDbContext _context;

        public LivestreamCartRepository(LivestreamDbContext context)
        {
            _context = context;
        }

        #region IGenericRepository Implementation

        public async Task<LivestreamCart?> GetByIdAsync(string id)
        {
            if (Guid.TryParse(id, out var guidId))
            {
                return await _context.LivestreamCarts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Id == guidId);
            }
            return null;
        }

        public async Task<LivestreamCart?> FindOneAsync(Expression<Func<LivestreamCart, bool>> filter)
        {
            return await _context.LivestreamCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<LivestreamCart>> FilterByAsync(Expression<Func<LivestreamCart, bool>> filter)
        {
            return await _context.LivestreamCarts
                .Include(c => c.Items)
                .Where(filter)
                .ToListAsync();
        }

        public async Task<IEnumerable<LivestreamCart>> GetAllAsync()
        {
            return await _context.LivestreamCarts
                .Include(c => c.Items)
                .ToListAsync();
        }

        public async Task InsertAsync(LivestreamCart entity)
        {
            await _context.LivestreamCarts.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task InsertManyAsync(IEnumerable<LivestreamCart> entities)
        {
            await _context.LivestreamCarts.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task ReplaceAsync(string id, LivestreamCart entity)
        {
            if (Guid.TryParse(id, out var guidId))
            {
                var existingEntity = await _context.LivestreamCarts.FindAsync(guidId);
                if (existingEntity != null)
                {
                    _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteAsync(string id)
        {
            if (Guid.TryParse(id, out var guidId))
            {
                var entity = await _context.LivestreamCarts.FindAsync(guidId);
                if (entity != null)
                {
                    _context.LivestreamCarts.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteManyAsync(IEnumerable<string> ids)
        {
            var guidIds = ids.Where(id => Guid.TryParse(id, out _))
                           .Select(id => Guid.Parse(id))
                           .ToList();

            var entities = await _context.LivestreamCarts
                .Where(c => guidIds.Contains(c.Id))
                .ToListAsync();

            _context.LivestreamCarts.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string id)
        {
            if (Guid.TryParse(id, out var guidId))
            {
                return await _context.LivestreamCarts.AnyAsync(c => c.Id == guidId);
            }
            return false;
        }

        public async Task<bool> ExistsAsync(Expression<Func<LivestreamCart, bool>> filter)
        {
            return await _context.LivestreamCarts.AnyAsync(filter);
        }

        public async Task<int> CountAsync(Expression<Func<LivestreamCart, bool>> filter)
        {
            return await _context.LivestreamCarts.CountAsync(filter);
        }

        public async Task<int> CountAsync()
        {
            return await _context.LivestreamCarts.CountAsync();
        }

        public async Task<PagedResult<LivestreamCart>> SearchAsync(
            string searchTerm,
            PaginationParams paginationParams,
            string[]? searchableFields = null,
            Expression<Func<LivestreamCart, bool>>? filter = null,
            bool exactMatch = false)
        {
            var query = _context.LivestreamCarts.Include(c => c.Items).AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Apply search term (basic implementation)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.ViewerId.ToString().Contains(searchTerm) ||
                                        c.LivestreamId.ToString().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResult<LivestreamCart>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / paginationParams.PageSize)
            };
        }

        #endregion

        #region ILivestreamCartRepository Specific Methods

        public async Task<LivestreamCart?> GetByLivestreamAndViewerAsync(Guid livestreamId, Guid viewerId)
        {
            return await _context.LivestreamCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.LivestreamId == livestreamId &&
                                        c.ViewerId == viewerId &&
                                        c.IsActive);
        }

        public async Task<LivestreamCart?> GetWithItemsAsync(Guid cartId)
        {
            return await _context.LivestreamCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
        }

        public async Task<IEnumerable<LivestreamCart>> GetExpiredCartsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.LivestreamCarts
                .Include(c => c.Items)
                .Where(c => c.ExpiresAt.HasValue &&
                           c.ExpiresAt < now &&
                           c.IsActive)
                .ToListAsync();
        }

        public async Task<int> CleanupExpiredCartsAsync()
        {
            var expiredCarts = await GetExpiredCartsAsync();
            int cleanedCount = 0;

            foreach (var cart in expiredCarts)
            {
                // Soft delete: deactivate instead of hard delete
                cart.Deactivate("system-cleanup");
                await _context.SaveChangesAsync();
                cleanedCount++;
            }

            return cleanedCount;
        }

        public async Task<int> CountActiveCartsInLivestreamAsync(Guid livestreamId)
        {
            return await _context.LivestreamCarts
                .CountAsync(c => c.LivestreamId == livestreamId &&
                               c.IsActive);
        }

        #endregion
    }
}