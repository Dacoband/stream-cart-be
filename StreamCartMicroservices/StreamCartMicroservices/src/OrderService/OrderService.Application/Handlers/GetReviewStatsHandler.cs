using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Queries;

namespace OrderService.Application.Handlers
{
    public class GetReviewStatsHandler : IRequestHandler<GetReviewStatsQuery, ReviewStatsDTO>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ILogger<GetReviewStatsHandler> _logger;

        public GetReviewStatsHandler(
            IReviewRepository reviewRepository,
            ILogger<GetReviewStatsHandler> logger)
        {
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task<ReviewStatsDTO> Handle(GetReviewStatsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var reviews = await _reviewRepository.GetByTargetAsync(request.TargetId, request.Type);

                var stats = new ReviewStatsDTO
                {
                    TargetId = request.TargetId,
                    Type = request.Type,
                    TotalReviews = reviews.Count(),
                    AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0,
                    RatingDistribution = reviews.GroupBy(r => r.Rating)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    VerifiedPurchaseCount = reviews.Count(r => r.IsVerifiedPurchase),
                    UnverifiedPurchaseCount = reviews.Count(r => !r.IsVerifiedPurchase),
                    LatestReviewDate = reviews.Any() ? reviews.Max(r => r.CreatedAt) : null,
                    OldestReviewDate = reviews.Any() ? reviews.Min(r => r.CreatedAt) : null,
                    TotalHelpfulVotes = reviews.Sum(r => r.HelpfulCount),
                    TotalUnhelpfulVotes = reviews.Sum(r => r.UnhelpfulCount),
                    ReviewsLast30Days = reviews.Count(r => r.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                };

                // Calculate quality score (simplified)
                if (reviews.Any())
                {
                    var avgTextLength = (decimal)reviews.Average(r => r.ReviewText.Length); // ✅ FIX: Convert to decimal
                    var verifiedRatio = (decimal)stats.VerifiedPurchaseCount / stats.TotalReviews;
                    stats.ReviewQualityScore = Math.Min(10, (avgTextLength / 50) + (verifiedRatio * 5));
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review stats for {TargetId}", request.TargetId);
                throw;
            }
        }
    }
}