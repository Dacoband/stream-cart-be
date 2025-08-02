using LivestreamService.Application.DTOs.Chat;
using MediatR;
using System;

namespace LivestreamService.Application.Queries.Chat
{
    public class GetChatMessageQuery1 : IRequest<ChatMessageDTO>
    {
        public Guid MessageId { get; set; }
    }
}