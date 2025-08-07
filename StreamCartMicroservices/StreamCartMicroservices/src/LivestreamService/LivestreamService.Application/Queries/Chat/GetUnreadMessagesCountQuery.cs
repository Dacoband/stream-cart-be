using MediatR;
using System;
using System.Collections.Generic;

namespace LivestreamService.Application.Queries.Chat
{
    public class GetUnreadMessagesCountQuery : IRequest<Dictionary<Guid, int>>
    {
        public Guid UserId { get; set; }
    }
}