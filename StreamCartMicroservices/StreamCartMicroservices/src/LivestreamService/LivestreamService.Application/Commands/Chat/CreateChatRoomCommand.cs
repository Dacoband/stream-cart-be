using LivestreamService.Application.DTOs.Chat;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Commands.Chat
{
    public class CreateChatRoomCommand : IRequest<ChatRoomDTO>
    {
        public Guid UserId { get; set; }
        public Guid ShopId { get; set; }
        public Guid? RelatedOrderId { get; set; }
        public string? InitialMessage { get; set; }
    }
}
