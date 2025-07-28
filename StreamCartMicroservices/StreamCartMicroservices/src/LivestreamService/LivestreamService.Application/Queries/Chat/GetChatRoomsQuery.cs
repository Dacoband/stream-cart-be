using LivestreamService.Application.DTOs.Chat;
using MediatR;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Queries.Chat
{
    public class GetChatRoomsQuery : IRequest<PagedResult<ChatRoomDTO>>
    {
        public Guid UserId { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
