using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using LivestreamService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using System.Linq.Expressions;

namespace LivestreamService.Infrastructure.Repositories
{
    public class LivestreamCartItemRepository : ILivestreamCartItemRepository
    {
        private readonly LivestreamDbContext _context;

        public LivestreamCartItemRepository(LivestreamDbContext context)
        {
            _context = context;
        }

        #region IGenericRepository Implementation

        public async Task<LivestreamCartItem?> GetByIdAsync(string id)
        {
            if (Guid.TryParse(id, out var guidId))
            {
                return await _context.LivestreamCartItems.FindAsync(guidId);
            }
            return null;
        }

        public async Task<LivestreamCartItem?> FindOneAsync(Expression<Func<LivestreamCartItem, bool>> filter)
        {
            return await _context.LivestreamCartItems.FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<LivestreamCartItem>> FilterByAsync(Expression<Func<LivestreamCartItem, bool>> filter)
        {
            return await _context.LivestreamCartItems.Where(filter).ToListAsync();
        }

        public async Task<IEnumerable<LivestreamCartItem>> GetAllAsync()
        {
            return await _context.LivestreamCartItems.ToListAsync();
        }

        public async Task InsertAsync(LivestreamCartItem entity)
        {
            await _context.LivestreamCartItems.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task InsertManyAsync(IEnumerable<LivestreamCartItem> entities)
        {
            await _context.LivestreamCartItems.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task ReplaceAsync(string id, LivestreamCartItem entity)
        {
            if (Guid.TryParse(id, out var guidId))
            {
                var existingEntity = await _context.LivestreamCartItems.FindAsync(guidId);
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
                var entity = await _context.LivestreamCartItems.FindAsync(guidId);
                if (entity != null)
                {
                    _context.LivestreamCartItems.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteManyAsync(IEnumerable<string> ids)
        {
            var guidIds = ids.Where(id => Guid.TryParse(id, out _))
                           .Select(id => Guid.Parse(id))
                           .ToList();

            var entities = await _context.LivestreamCartItems
                .Where(i => guidIds.Contains(i.Id))
                .ToListAsync();

            _context.LivestreamCartItems.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string id)
        {
            if (Guid.TryParse(id, out var guidId))
            {
                return await _context.LivestreamCartItems.AnyAsync(i => i.Id == guidId);
            }
            return false;
        }

        public async Task<bool> ExistsAsync(Expression<Func<LivestreamCartItem, bool>> filter)
        {
            return await _context.LivestreamCartItems.AnyAsync(filter);
        }

        public async Task<int> CountAsync(Expression<Func<LivestreamCartItem, bool>> filter)
        {
            return await _context.LivestreamCartItems.CountAsync(filter);
        }

        public async Task<int> CountAsync()
        {
            return await _context.LivestreamCartItems.CountAsync();
        }

        public async Task<PagedResult<LivestreamCartItem>> SearchAsync(
            string searchTerm,
            PaginationParams paginationParams,
            string[]? searchableFields = null,
            Expression<Func<LivestreamCartItem, bool>>? filter = null,
            bool exactMatch = false)
        {
            var query = _context.LivestreamCartItems.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(i => i.ProductName.Contains(searchTerm) ||
                                        i.ProductId.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResult<LivestreamCartItem>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / paginationParams.PageSize)
            };
        }

        #endregion

        #region ILivestreamCartItemRepository Specific Methods

        public async Task<IEnumerable<LivestreamCartItem>> GetByCartIdAsync(Guid cartId)
        {
            return await _context.LivestreamCartItems
                .Where(item => item.LivestreamCartId == cartId)
                .OrderByDescending(item => item.CreatedAt)
                .ToListAsync();
        }

        public async Task<LivestreamCartItem?> FindByCartAndProductAsync(Guid cartId, Guid livestreamProductId, string? variantId = null)
        {
            var query = _context.LivestreamCartItems
                .Where(item => item.LivestreamCartId == cartId &&
                             item.LivestreamProductId == livestreamProductId);

            if (!string.IsNullOrEmpty(variantId))
            {
                query = query.Where(item => item.VariantId == variantId);
            }
            else
            {
                query = query.Where(item => item.VariantId == null || item.VariantId == "");
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LivestreamCartItem>> GetByLivestreamProductIdAsync(Guid livestreamProductId)
        {
            return await _context.LivestreamCartItems
                .Where(item => item.LivestreamProductId == livestreamProductId)
                .ToListAsync();
        }

        public async Task DeleteCartItemAsync(Guid cartItemId)
        {
            var item = await _context.LivestreamCartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.LivestreamCartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateStockForLivestreamProductAsync(Guid livestreamProductId, int newStock)
        {
            var items = await GetByLivestreamProductIdAsync(livestreamProductId);

            foreach (var item in items)
            {
                item.Stock = newStock;
                item.SetModifier("system-stock-update");
            }

            if (items.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        #endregion
    }
}