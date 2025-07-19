using LivestreamService.Application.DTOs.Chat;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Queries.Chat
{
    public class GetChatRoomQuery : IRequest<ChatRoomDTO?>
    {
        public Guid UserId { get; set; }
        public Guid ShopId { get; set; }
    }
}
