using LivestreamService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface IStreamViewRepository : IGenericRepository<StreamView>
    {
        Task<IEnumerable<StreamView>> GetByLivestreamIdAsync(Guid livestreamId);
        Task<IEnumerable<StreamView>> GetByUserIdAsync(Guid userId);
        Task<StreamView?> GetActiveViewByUserAsync(Guid livestreamId, Guid userId);
        Task<int> CountActiveViewersAsync(Guid livestreamId);
        Task<int> CountTotalViewsAsync(Guid livestreamId);
        Task<int> CountUniqueViewersAsync(Guid livestreamId);
        Task<TimeSpan> GetAverageViewDurationAsync(Guid livestreamId);
        Task<IEnumerable<StreamView>> GetViewsWithinTimeRangeAsync(Guid livestreamId, DateTime startTime, DateTime endTime);
    }
}