using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using LivestreamService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Repositories
{
    public class LivestreamRepository : ILivestreamRepository
    {
        private readonly LivestreamDbContext _context;
        private readonly ILogger<LivestreamRepository> _logger;

        public LivestreamRepository(
            LivestreamDbContext context,
            ILogger<LivestreamRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Existing methods...
        public async Task<Livestream> GetByIdAsync(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid livestreamId))
                {
                    _logger.LogWarning("Invalid livestream ID: {LivestreamId}", id);
                    return null;
                }

                return await _context.Livestreams
                    .FirstOrDefaultAsync(l => l.Id == livestreamId && !l.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream by ID {LivestreamId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Livestream>> GetLivestreamsBySellerIdAsync(Guid sellerId)
        {
            try
            {
                return await _context.Livestreams
                    .Where(l => l.SellerId == sellerId && !l.IsDeleted)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestreams by seller ID {SellerId}", sellerId);
                throw;
            }
        }

        public async Task<IEnumerable<Livestream>> GetActiveLivestreamsAsync()
        {
            try
            {
                return await _context.Livestreams
                    .Where(l => l.Status && !l.IsDeleted)
                    .OrderByDescending(l => l.ActualStartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active livestreams");
                throw;
            }
        }

        public async Task<IEnumerable<Livestream>> GetUpcomingLivestreamsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                return await _context.Livestreams
                    .Where(l => !l.Status && l.ScheduledStartTime > now && !l.IsDeleted)
                    .OrderBy(l => l.ScheduledStartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming livestreams");
                throw;
            }
        }

        public async Task<IEnumerable<Livestream>> GetLivestreamsByShopIdAsync(Guid shopId)
        {
            try
            {
                return await _context.Livestreams
                    .Where(l => l.ShopId == shopId && !l.IsDeleted)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestreams by shop ID {ShopId}", shopId);
                throw;
            }
        }

        public async Task<IEnumerable<Livestream>> GetAllAsync()
        {
            try
            {
                return await _context.Livestreams
                    .Where(l => !l.IsDeleted)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all livestreams");
                throw;
            }
        }

        public async Task<PagedResult<Livestream>> GetPagedLivestreamsAsync(
            int pageNumber,
            int pageSize,
            bool activeOnly = false,
            bool promotedOnly = false,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string searchTerm = null)
        {
            try
            {
                IQueryable<Livestream> query = _context.Livestreams.Where(l => !l.IsDeleted);

                if (activeOnly)
                {
                    query = query.Where(l => l.Status);
                }

                if (promotedOnly)
                {
                    query = query.Where(l => l.IsPromoted);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(l => l.ScheduledStartTime >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(l => l.ScheduledStartTime <= endDate.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var normalizedSearchTerm = searchTerm.ToLower();
                    query = query.Where(l =>
                        l.Title.ToLower().Contains(normalizedSearchTerm) ||
                        l.Description.ToLower().Contains(normalizedSearchTerm) ||
                        l.Tags.ToLower().Contains(normalizedSearchTerm));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<Livestream>
                {
                    Items = items,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged livestreams");
                throw;
            }
        }

        public async Task InsertAsync(Livestream livestream)
        {
            try
            {
                await _context.Livestreams.AddAsync(livestream);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting livestream");
                throw;
            }
        }

        public async Task ReplaceAsync(string id, Livestream livestream)
        {
            try
            {
                _context.Entry(livestream).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing livestream {LivestreamId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid livestreamId))
                {
                    throw new ArgumentException("Invalid ID format", nameof(id));
                }

                var livestream = await _context.Livestreams.FindAsync(livestreamId);
                if (livestream != null)
                {
                    livestream.Delete();
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting livestream {LivestreamId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid livestreamId))
                {
                    return false;
                }

                return await _context.Livestreams
                    .AnyAsync(l => l.Id == livestreamId && !l.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if livestream exists with ID {LivestreamId}", id);
                throw;
            }
        }

        // Missing methods from IGenericRepository<Livestream>

        // Method 1: FindOneAsync
        public async Task<Livestream> FindOneAsync(Expression<Func<Livestream, bool>> filter)
        {
            try
            {
                return await _context.Livestreams
                    .Where(l => !l.IsDeleted)
                    .FirstOrDefaultAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding livestream with filter");
                throw;
            }
        }

        // Method 2: FilterByAsync
        public async Task<IEnumerable<Livestream>> FilterByAsync(Expression<Func<Livestream, bool>> filter)
        {
            try
            {
                return await _context.Livestreams
                    .Where(l => !l.IsDeleted)
                    .Where(filter)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering livestreams");
                throw;
            }
        }

        // Method 3: InsertManyAsync
        public async Task InsertManyAsync(IEnumerable<Livestream> entities)
        {
            try
            {
                await _context.Livestreams.AddRangeAsync(entities);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting multiple livestreams");
                throw;
            }
        }

        // Method 4: DeleteManyAsync
        public async Task DeleteManyAsync(IEnumerable<string> ids)
        {
            try
            {
                var validIds = new List<Guid>();
                foreach (var id in ids)
                {
                    if (Guid.TryParse(id, out Guid guid))
                    {
                        validIds.Add(guid);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid livestream ID: {LivestreamId}", id);
                    }
                }

                var livestreams = await _context.Livestreams
                    .Where(l => validIds.Contains(l.Id) && !l.IsDeleted)
                    .ToListAsync();

                foreach (var livestream in livestreams)
                {
                    livestream.Delete();
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting multiple livestreams");
                throw;
            }
        }

        // Method 5: ExistsAsync overload
        public async Task<bool> ExistsAsync(Expression<Func<Livestream, bool>> filter)
        {
            try
            {
                return await _context.Livestreams
                    .Where(l => !l.IsDeleted)
                    .AnyAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if livestream exists with filter");
                throw;
            }
        }

        // Method 6: CountAsync with filter
        public async Task<int> CountAsync(Expression<Func<Livestream, bool>> filter)
        {
            try
            {
                return await _context.Livestreams
                    .Where(l => !l.IsDeleted)
                    .CountAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting livestreams with filter");
                throw;
            }
        }

        // Method 7: CountAsync without filter
        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Livestreams
                    .Where(l => !l.IsDeleted)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting all livestreams");
                throw;
            }
        }

        // Method 8: SearchAsync
        public async Task<PagedResult<Livestream>> SearchAsync(
            string searchText,
            PaginationParams paginationParams,
            string[]? searchFields = null,
            Expression<Func<Livestream, bool>>? filter = null,
            bool exactMatch = false)
        {
            try
            {
                IQueryable<Livestream> query = _context.Livestreams.Where(l => !l.IsDeleted);

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                if (!string.IsNullOrEmpty(searchText) && searchFields != null && searchFields.Any())
                {
                    if (exactMatch)
                    {
                        // Implement exact match search
                        var predicate = PredicateBuilder.False<Livestream>();
                        foreach (var field in searchFields)
                        {
                            if (field == "Title")
                                predicate = predicate.Or(l => l.Title == searchText);
                            else if (field == "Description")
                                predicate = predicate.Or(l => l.Description == searchText);
                            else if (field == "Tags")
                                predicate = predicate.Or(l => l.Tags == searchText);
                        }
                        query = query.Where(predicate);
                    }
                    else
                    {
                        // Implement contains search
                        var predicate = PredicateBuilder.False<Livestream>();
                        foreach (var field in searchFields)
                        {
                            if (field == "Title")
                                predicate = predicate.Or(l => l.Title.Contains(searchText));
                            else if (field == "Description")
                                predicate = predicate.Or(l => l.Description.Contains(searchText));
                            else if (field == "Tags")
                                predicate = predicate.Or(l => l.Tags.Contains(searchText));
                        }
                        query = query.Where(predicate);
                    }
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToListAsync();

                return new PagedResult<Livestream>
                {
                    Items = items,
                    CurrentPage = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching livestreams");
                throw;
            }
        }
    }

    // Helper class for building predicates
    internal static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}