using LivestreamService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface ILivestreamChatRepository : IGenericRepository<LivestreamChat>
    {
        Task<PagedResult<LivestreamChat>> GetLivestreamChatAsync(
            Guid livestreamId,
            int pageNumber,
            int pageSize,
            bool includeModerated = false);

        Task<IEnumerable<LivestreamChat>> GetRecentMessagesAsync(
            Guid livestreamId,
            int limit = 50);

        Task<int> GetUnmoderatedMessageCountAsync(Guid livestreamId);

        Task<IEnumerable<LivestreamChat>> GetMessagesByUserAsync(
            Guid livestreamId,
            Guid userId);
    }
}
