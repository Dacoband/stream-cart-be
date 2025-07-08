using LivestreamService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface ILivestreamRepository : IGenericRepository<Livestream>
    {
        Task<Livestream> GetByIdAsync(string id);
        Task<IEnumerable<Livestream>> GetLivestreamsBySellerIdAsync(Guid sellerId);
        Task<IEnumerable<Livestream>> GetActiveLivestreamsAsync();
        Task<IEnumerable<Livestream>> GetUpcomingLivestreamsAsync();
        Task<IEnumerable<Livestream>> GetLivestreamsByShopIdAsync(Guid shopId);
        Task<IEnumerable<Livestream>> GetAllAsync();
        Task<PagedResult<Livestream>> GetPagedLivestreamsAsync(
            int pageNumber,
            int pageSize,
            bool activeOnly = false,
            bool promotedOnly = false,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string searchTerm = null);
        Task InsertAsync(Livestream livestream);
        Task ReplaceAsync(string id, Livestream livestream);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}