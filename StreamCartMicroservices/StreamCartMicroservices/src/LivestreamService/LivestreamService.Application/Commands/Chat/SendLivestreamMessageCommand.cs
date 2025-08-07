using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Domain.Entities;
using LivestreamService.Domain.Enums;
using MediatR;
using System;

namespace LivestreamService.Application.Commands.Chat
{
    public class SendLivestreamMessageCommand : IRequest<LivestreamChatDTO>
    {
        public Guid LivestreamId { get; set; }
        public Guid SenderId { get; set; }
        public string Message { get; set; } = string.Empty;
        public MessageType MessageType { get; set; } = MessageType.Text;
        public Guid? ReplyToMessageId { get; set; }
    }

    public class ModerateLivestreamMessageCommand : IRequest<LivestreamChatDTO>
    {
        public Guid MessageId { get; set; }
        public bool IsModerated { get; set; }
        public Guid ModeratorId { get; set; }
    }
}