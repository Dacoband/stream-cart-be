using LivestreamService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface IStreamEventRepository : IGenericRepository<StreamEvent>
    {
        Task<IEnumerable<StreamEvent>> GetByLivestreamIdAsync(Guid livestreamId);
        Task<IEnumerable<StreamEvent>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<StreamEvent>> GetRecentEventsByLivestreamAsync(Guid livestreamId, int count = 50);
        Task<IEnumerable<StreamEvent>> GetEventsByTypeAsync(Guid livestreamId, string eventType);
        Task<int> CountEventsByTypeAsync(Guid livestreamId, string eventType);
        Task<IEnumerable<StreamEvent>> GetEventsByProductAsync(Guid livestreamProductId);
    }
}