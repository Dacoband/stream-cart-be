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
    public class GetChatMessagesQuery : IRequest<PagedResult<ChatMessageDTO>>
    {
        public Guid ChatRoomId { get; set; }
        public Guid RequesterId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
