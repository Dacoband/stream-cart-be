using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Shared.Common.Data.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(string id);
        Task<T?> FindOneAsync(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> FilterByAsync(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> GetAllAsync();
        Task InsertAsync(T entity);
        Task InsertManyAsync(IEnumerable<T> entities);
        /// <summary>
        /// Replaces an existing entity with the specified ID in the repository.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task ReplaceAsync(string id, T entity);
        Task DeleteAsync(string id);
        Task DeleteManyAsync(IEnumerable<string> ids);
        /// <summary>
        /// Checks if an entity with the specified ID exists in the repository.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string id);
        /// <summary>
        /// Checks if any entity matches the specified filter criteria.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
        /// <summary>
        /// Counts the number of entities that match the specified filter criteria.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<int> CountAsync(Expression<Func<T, bool>> filter);
        /// <summary>
        /// Counts the total number of entities in the repository.
        /// </summary>
        /// <returns></returns>
        Task<int> CountAsync();
        /// <summary>
        /// Searches for entities based on search criteria with pagination support.
        /// </summary>
        /// <param name="searchTerm">The text to search for across searchable fields</param>
        /// <param name="paginationParams">Pagination parameters (page number and page size)</param>
        /// <param name="searchableFields">Optional array of field names to search in. If null, searches in all text fields.</param>
        /// <param name="exactMatch">If true, searches for exact matches. If false, uses contains/partial matching.</param>
        /// <param name="filter">Optional filter to apply in addition to the search term</param>
        /// <returns>A paged result containing the matching entities</returns>
        Task<PagedResult<T>> SearchAsync(
            string searchTerm,
            PaginationParams paginationParams,
            string[]? searchableFields = null,
            Expression<Func<T, bool>>? filter = null,
            bool exactMatch = false);
    }
}