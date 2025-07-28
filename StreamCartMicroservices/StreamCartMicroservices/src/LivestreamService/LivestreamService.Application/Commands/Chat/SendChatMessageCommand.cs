using LivestreamService.Application.DTOs.Chat;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Commands.Chat
{
    public class SendChatMessageCommand : IRequest<ChatMessageDTO>
    {
        public Guid ChatRoomId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text";
        public string? AttachmentUrl { get; set; }
    }
}
