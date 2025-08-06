using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using ShopService.Infrastructure.Data;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShopService.Infrastructure.Repositories
{
    public class ShopDashboardRepository : IShopDashboardRepository
    {
        private readonly ShopContext _context;
        private readonly ILogger<ShopDashboardRepository> _logger;

        public ShopDashboardRepository(
            ShopContext context,
            ILogger<ShopDashboardRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ShopDashboard?> GetLatestDashboardAsync(Guid shopId, string periodType)
        {
            try
            {
                return await _context.ShopDashboards
                    .Where(d => d.ShopId == shopId &&
                           d.PeriodType == periodType &&
                           !d.IsDeleted)
                    .OrderByDescending(d => d.ToTime)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest dashboard for shop {ShopId}", shopId);
                throw;
            }
        }

        public async Task<ShopDashboard?> GetDashboardByPeriodAsync(Guid shopId, DateTime fromDate, DateTime toDate, string periodType)
        {
            try
            {
                return await _context.ShopDashboards
                    .Where(d => d.ShopId == shopId &&
                           d.PeriodType == periodType &&
                           d.FromTime == fromDate &&
                           d.ToTime == toDate &&
                           !d.IsDeleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard by period for shop {ShopId}", shopId);
                throw;
            }
        }

        public async Task<IEnumerable<ShopDashboard>> GetDashboardHistoryAsync(Guid shopId, DateTime fromDate, DateTime toDate, string periodType, int limit = 10)
        {
            try
            {
                return await _context.ShopDashboards
                    .Where(d => d.ShopId == shopId &&
                           d.PeriodType == periodType &&
                           d.FromTime >= fromDate &&
                           d.ToTime <= toDate &&
                           !d.IsDeleted)
                    .OrderByDescending(d => d.ToTime)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard history for shop {ShopId}", shopId);
                throw;
            }
        }

        public async Task<PagedResult<ShopDashboard>> GetPagedDashboardsAsync(Guid shopId, int pageNumber, int pageSize, string? periodType = null)
        {
            try
            {
                var query = _context.ShopDashboards
                    .Where(d => d.ShopId == shopId && !d.IsDeleted);

                if (!string.IsNullOrEmpty(periodType))
                {
                    query = query.Where(d => d.PeriodType == periodType);
                }

                var totalCount = await query.CountAsync();

                var skip = (pageNumber - 1) * pageSize;
                var dashboards = await query
                    .OrderByDescending(d => d.ToTime)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<ShopDashboard>(dashboards, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged dashboards for shop {ShopId}", shopId);
                throw;
            }
        }

        #region IGenericRepository Implementation

        public async Task<ShopDashboard?> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var dashboardId))
                return null;

            return await _context.ShopDashboards
                .FirstOrDefaultAsync(d => d.Id == dashboardId && !d.IsDeleted);
        }

        public async Task<ShopDashboard?> FindOneAsync(Expression<Func<ShopDashboard, bool>> filter)
        {
            return await _context.ShopDashboards
                .Where(d => !d.IsDeleted)
                .FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<ShopDashboard>> FilterByAsync(Expression<Func<ShopDashboard, bool>> filter)
        {
            return await _context.ShopDashboards
                .Where(d => !d.IsDeleted)
                .Where(filter)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShopDashboard>> GetAllAsync()
        {
            return await _context.ShopDashboards
                .Where(d => !d.IsDeleted)
                .ToListAsync();
        }

        public async Task InsertAsync(ShopDashboard entity)
        {
            await _context.ShopDashboards.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task InsertManyAsync(IEnumerable<ShopDashboard> entities)
        {
            await _context.ShopDashboards.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task ReplaceAsync(string id, ShopDashboard entity)
        {
            if (!Guid.TryParse(id, out var dashboardId) || entity.Id != dashboardId)
                throw new ArgumentException("Invalid ID", nameof(id));

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var dashboardId))
                throw new ArgumentException("Invalid ID", nameof(id));

            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.Delete();
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteManyAsync(IEnumerable<string> ids)
        {
            var guidIds = ids.Select(id => {
                if (Guid.TryParse(id, out var guid))
                    return guid;
                throw new ArgumentException($"Invalid ID: {id}");
            });

            var entities = await _context.ShopDashboards
                .Where(d => guidIds.Contains(d.Id))
                .ToListAsync();

            foreach (var entity in entities)
            {
                entity.Delete();
                _context.Entry(entity).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string id)
        {
            if (!Guid.TryParse(id, out var dashboardId))
                return false;

            return await _context.ShopDashboards
                .AnyAsync(d => d.Id == dashboardId && !d.IsDeleted);
        }

        public async Task<bool> ExistsAsync(Expression<Func<ShopDashboard, bool>> filter)
        {
            return await _context.ShopDashboards
                .Where(d => !d.IsDeleted)
                .AnyAsync(filter);
        }

        public async Task<int> CountAsync(Expression<Func<ShopDashboard, bool>> filter)
        {
            return await _context.ShopDashboards
                .Where(d => !d.IsDeleted)
                .CountAsync(filter);
        }

        public async Task<int> CountAsync()
        {
            return await _context.ShopDashboards
                .Where(d => !d.IsDeleted)
                .CountAsync();
        }

        public async Task<PagedResult<ShopDashboard>> SearchAsync(
            string searchTerm,
            PaginationParams paginationParams,
            string[]? searchableFields = null,
            Expression<Func<ShopDashboard, bool>>? filter = null,
            bool exactMatch = false)
        {
            var query = _context.ShopDashboards.Where(d => !d.IsDeleted);

            // Apply additional filter if provided
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Apply search logic (simplified for example)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(d => d.Notes.Contains(searchTerm));
            }

            // Count total results
            var totalCount = await query.CountAsync();

            // Apply pagination
            var skip = (paginationParams.PageNumber - 1) * paginationParams.PageSize;
            var items = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip(skip)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResult<ShopDashboard>(
                items,
                totalCount,
                paginationParams.PageNumber,
                paginationParams.PageSize);
        }

        #endregion
    }
}