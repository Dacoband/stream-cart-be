using MediatR;
using ShopService.Application.DTOs;
using ShopService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System.Collections.Generic;

namespace ShopService.Application.Queries
{
    public class SearchShopsQuery : IRequest<PagedResult<ShopDto>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ShopStatus? Status { get; set; }
        public ApprovalStatus? ApprovalStatus { get; set; }
        public string? SortBy { get; set; }
        public bool Ascending { get; set; } = true;
    }
}