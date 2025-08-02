using LivestreamService.Application.DTOs.Chat;
using MediatR;
using Shared.Common.Domain.Bases;
using System;

namespace LivestreamService.Application.Queries.Chat
{
    public class GetLivestreamChatHistoryQuery : IRequest<PagedResult<LivestreamChatDTO>>
    {
        public Guid LivestreamId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public bool IncludeModerated { get; set; } = false;
    }
}