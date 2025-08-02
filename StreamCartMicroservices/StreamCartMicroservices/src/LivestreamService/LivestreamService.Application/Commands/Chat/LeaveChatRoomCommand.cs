using MediatR;
using System;

namespace LivestreamService.Application.Commands.Chat
{
    public class LeaveChatRoomCommand : IRequest<bool>
    {
        public Guid ChatRoomId { get; set; }
        public Guid UserId { get; set; }
    }
}