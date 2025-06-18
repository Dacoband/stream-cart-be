using Microsoft.EntityFrameworkCore;
using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace Shared.Common.Data.Repositories
{
    public class EfCoreGenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public EfCoreGenericRepository(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            if (Guid.TryParse(id, out Guid guidId))
            {
                return await _dbSet.FindAsync(guidId);
            }
            return null;
        }

        public async Task<T?> FindOneAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<T>> FilterByAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.Where(filter).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task InsertAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertManyAsync(IEnumerable<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            
            await _dbSet.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
        }

        public async Task ReplaceAsync(string id, T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            if (Guid.TryParse(id, out Guid guidId))
            {
                var existingEntity = await _dbSet.FindAsync(guidId);
                if (existingEntity == null)
                {
                    throw new KeyNotFoundException($"Entity with ID {id} not found.");
                }

                _dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Invalid ID format.", nameof(id));
            }
        }

        public async Task DeleteAsync(string id)
        {
            if (Guid.TryParse(id, out Guid guidId))
            {
                var entity = await _dbSet.FindAsync(guidId);
                if (entity != null)
                {   
                    entity.Delete(); // Using soft delete from BaseEntity
                    _dbContext.Entry(entity).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteManyAsync(IEnumerable<string> ids)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            
            foreach (var id in ids)
            {
                await DeleteAsync(id);
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            if (Guid.TryParse(id, out Guid guidId))
            {
                return await _dbSet.AnyAsync(e => e.Id == guidId);
            }
            return false;
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.AnyAsync(filter);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.CountAsync(filter);
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<PagedResult<T>> SearchAsync(
            string searchTerm,
            PaginationParams paginationParams,
            string[]? searchableFields = null,
            Expression<Func<T, bool>>? filter = null,
            bool exactMatch = false)
        {
            // Start with the entire collection
            IQueryable<T> query = _dbSet;
            
            // Apply additional filter if provided
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Search implementation would depend on specific entity properties
            // This is a simple placeholder - in real implementation you'd use reflection
            // or EF.Functions.Like for text search based on searchableFields

            // Get total count for pagination
            var totalCount = await query.CountAsync();
            
            // Apply pagination
            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();
            
            // Create and return paged result
            return new PagedResult<T>(
                items, 
                totalCount, 
                paginationParams.PageNumber, 
                paginationParams.PageSize
            );
        }
    }
}