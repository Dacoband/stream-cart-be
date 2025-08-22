using ReviewService.Domain.Enums;

namespace OrderService.Application.DTOs
{
    public class ReviewStatsDTO
    {
        public Guid TargetId { get; set; }
        public ReviewType Type { get; set; }
        public int TotalReviews { get; set; }
        public decimal AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public int VerifiedPurchaseCount { get; set; }
        public int UnverifiedPurchaseCount { get; set; }
        public DateTime? LatestReviewDate { get; set; }
        public DateTime? OldestReviewDate { get; set; }
        public int TotalHelpfulVotes { get; set; }
        public int TotalUnhelpfulVotes { get; set; }

        // Quality metrics
        public decimal ReviewQualityScore { get; set; } // Based on length, verified purchase, etc.
        public decimal ResponseRate { get; set; } // Percentage of reviews with responses

        // Trending data
        public decimal RatingTrend { get; set; } // Positive/negative trend over last 30 days
        public int ReviewsLast30Days { get; set; }
    }
}