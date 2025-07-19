using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Commands.Chat
{
    public class MarkMessagesAsReadCommand : IRequest<bool>
    {
        public Guid ChatRoomId { get; set; }
        public Guid UserId { get; set; }
    }
}
