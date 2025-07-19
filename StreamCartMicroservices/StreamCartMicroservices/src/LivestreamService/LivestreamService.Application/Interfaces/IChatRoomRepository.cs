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
    public interface IChatRoomRepository : IGenericRepository<ChatRoom>
    {
        Task<ChatRoom?> GetByUserAndShopAsync(Guid userId, Guid shopId);
        Task<PagedResult<ChatRoom>> GetUserChatRoomsAsync(
            Guid userId,
            int pageNumber,
            int pageSize,
            bool? isActive = null);
        Task<PagedResult<ChatRoom>> GetShopChatRoomsAsync(
            Guid shopId,
            int pageNumber,
            int pageSize,
            bool? isActive = null);
    }

    public interface IChatMessageRepository : IGenericRepository<ChatMessage>
    {
        Task<PagedResult<ChatMessage>> GetChatRoomMessagesAsync(
            Guid chatRoomId,
            int pageNumber,
            int pageSize);

        Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(
            Guid chatRoomId,
            Guid userId);

        Task<int> GetUnreadCountAsync(Guid chatRoomId, Guid userId);

        Task MarkMessagesAsReadAsync(Guid chatRoomId, Guid userId);

        Task<ChatMessage?> GetLastMessageAsync(Guid chatRoomId);
    }
}
