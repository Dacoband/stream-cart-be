using MediatR;
using OrderService.Application.DTOs;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Queries
{
    public class GetReviewsByProductQuery : IRequest<PagedResult<ReviewDTO>>
    {
        public Guid ProductId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public bool? VerifiedPurchaseOnly { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool Ascending { get; set; } = false;
    }
}
