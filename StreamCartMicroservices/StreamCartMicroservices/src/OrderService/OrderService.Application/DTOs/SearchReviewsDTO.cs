using ReviewService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs
{
    public class SearchReviewsDTO
    {
        public string? SearchTerm { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        public ReviewType? Type { get; set; }
        public Guid? TargetId { get; set; }
        public Guid? UserId { get; set; }

        [Range(1, 5, ErrorMessage = "Minimum rating must be between 1 and 5")]
        public int? MinRating { get; set; }

        [Range(1, 5, ErrorMessage = "Maximum rating must be between 1 and 5")]
        public int? MaxRating { get; set; }

        public bool? VerifiedPurchaseOnly { get; set; }
        public bool? HasImages { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? SortBy { get; set; } = "CreatedAt";
        public bool Ascending { get; set; } = false;

        // Advanced filters
        public int? MinHelpfulVotes { get; set; }
        public bool? HasResponse { get; set; }
        public int? MinTextLength { get; set; }
    }
}