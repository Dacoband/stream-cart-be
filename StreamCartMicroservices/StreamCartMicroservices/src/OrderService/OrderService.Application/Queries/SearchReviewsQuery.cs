using MediatR;
using OrderService.Application.DTOs;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;

namespace OrderService.Application.Queries
{
    public class SearchReviewsQuery : IRequest<PagedResult<ReviewDTO>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ReviewService.Domain.Enums.ReviewType? Type { get; set; }
        public Guid? TargetId { get; set; }
        public Guid? UserId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public bool? VerifiedPurchaseOnly { get; set; }
        public bool? HasImages { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool Ascending { get; set; } = false;
        public int? MinHelpfulVotes { get; set; }
        public bool? HasResponse { get; set; }
        public int? MinTextLength { get; set; }
    }
}