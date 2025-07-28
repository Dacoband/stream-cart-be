using LivestreamService.Application.DTOs.Chat;
using MediatR;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using System;

namespace LivestreamService.Application.Queries.Chat
{
    public class GetShopChatRoomsQuery : IRequest<PagedResult<ChatRoomDTO>>
    {
        public Guid ShopId { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}