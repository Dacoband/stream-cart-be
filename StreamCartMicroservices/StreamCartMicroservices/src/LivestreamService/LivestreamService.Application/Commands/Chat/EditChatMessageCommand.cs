using LivestreamService.Application.DTOs.Chat;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Commands.Chat
{
    public class EditChatMessageCommand : IRequest<ChatMessageDTO>
    {
        public Guid MessageId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
