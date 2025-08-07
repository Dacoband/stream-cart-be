using LivestreamService.Application.DTOs.Chat;
using MediatR;
using System;

namespace LivestreamService.Application.Queries.Chat
{
    public class GetLivestreamMessageByIdQuery : IRequest<LivestreamChatDTO>
    {
        public Guid MessageId { get; set; }
    }
}