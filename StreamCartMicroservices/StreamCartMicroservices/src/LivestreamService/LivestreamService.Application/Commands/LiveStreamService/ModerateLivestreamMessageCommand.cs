using LivestreamService.Application.DTOs.Chat;
using MediatR;
using System;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class ModerateLivestreamMessageCommand : IRequest<LivestreamChatDTO>
    {
        public Guid MessageId { get; set; }
        public bool IsModerated { get; set; }
        public Guid ModeratedBy { get; set; }
    }
}