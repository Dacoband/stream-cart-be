using LivestreamService.Application.DTOs.Chat;
using MediatR;
using Shared.Common.Domain.Bases;
using System;

namespace LivestreamService.Application.Queries.Chat
{
    public class SearchChatMessagesQuery : IRequest<PagedResult<ChatMessageDTO>>
    {
        public Guid ChatRoomId { get; set; }
        public Guid RequesterId { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}