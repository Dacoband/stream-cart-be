using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Data;
using ReviewService.Domain.Enums;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;

namespace OrderService.Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly OrderContext _context;

        public ReviewRepository(OrderContext context)
        {
            _context = context;
        }

        public async Task<Review?> GetByIdAsync(Guid id)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId)
        {
            return await _context.Reviews
                .Where(r => r.ProductID == productId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Reviews
                .Where(r => r.OrderID == orderId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByLivestreamIdAsync(Guid livestreamId)
        {
            return await _context.Reviews
                .Where(r => r.LivestreamId == livestreamId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Reviews
                .Where(r => r.AccountID == userId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByTargetAsync(Guid targetId, ReviewType type)
        {
            return type switch
            {
                ReviewType.Product => await GetByProductIdAsync(targetId),
                ReviewType.Order => await GetByOrderIdAsync(targetId),
                ReviewType.Livestream => await GetByLivestreamIdAsync(targetId),
                _ => throw new ArgumentException("Invalid review type")
            };
        }

        public async Task<PagedResult<Review>> GetByProductIdPagedAsync(
            Guid productId,
            int pageNumber,
            int pageSize,
            int? minRating = null,
            int? maxRating = null,
            bool? verifiedPurchaseOnly = null,
            string? sortBy = null,
            bool ascending = false)
        {
            var query = _context.Reviews
                .Where(r => r.ProductID == productId && !r.IsDeleted);

            if (minRating.HasValue)
                query = query.Where(r => r.Rating >= minRating.Value);

            if (maxRating.HasValue)
                query = query.Where(r => r.Rating <= maxRating.Value);

            if (verifiedPurchaseOnly.HasValue && verifiedPurchaseOnly.Value)
                query = query.Where(r => r.IsVerifiedPurchase);

            // Sorting
            query = sortBy?.ToLower() switch
            {
                "rating" => ascending ? query.OrderBy(r => r.Rating) : query.OrderByDescending(r => r.Rating),
                "helpful" => ascending ? query.OrderBy(r => r.HelpfulCount) : query.OrderByDescending(r => r.HelpfulCount),
                _ => ascending ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Review>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<Review>> SearchAsync(
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 10,
            ReviewType? type = null,
            Guid? targetId = null,
            Guid? userId = null,
            int? minRating = null,
            int? maxRating = null,
            bool? verifiedPurchaseOnly = null,
            bool? hasImages = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? sortBy = null,
            bool ascending = false,
            int? minHelpfulVotes = null,
            bool? hasResponse = null,
            int? minTextLength = null)
        {
            var query = _context.Reviews.Where(r => !r.IsDeleted);

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => r.ReviewText.Contains(searchTerm));
            }

            if (type.HasValue)
            {
                query = query.Where(r => r.Type == type.Value);
            }

            if (targetId.HasValue)
            {
                query = type switch
                {
                    ReviewType.Product => query.Where(r => r.ProductID == targetId),
                    ReviewType.Order => query.Where(r => r.OrderID == targetId),
                    ReviewType.Livestream => query.Where(r => r.LivestreamId == targetId),
                    _ => query
                };
            }

            if (userId.HasValue)
            {
                query = query.Where(r => r.AccountID == userId.Value);
            }

            if (minRating.HasValue)
            {
                query = query.Where(r => r.Rating >= minRating.Value);
            }

            if (maxRating.HasValue)
            {
                query = query.Where(r => r.Rating <= maxRating.Value);
            }

            if (verifiedPurchaseOnly.HasValue && verifiedPurchaseOnly.Value)
            {
                query = query.Where(r => r.IsVerifiedPurchase);
            }

            if (hasImages.HasValue)
            {
                if (hasImages.Value)
                {
                    query = query.Where(r => r.ImageUrl != null);
                }
                else
                {
                    query = query.Where(r => r.ImageUrl == null);
                }
            }

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= toDate.Value);
            }

            if (minHelpfulVotes.HasValue)
            {
                query = query.Where(r => r.HelpfulCount >= minHelpfulVotes.Value);
            }

            if (hasResponse.HasValue)
            {
                // This would need to be implemented based on your response mechanism
                // For now, we'll skip this filter
            }

            if (minTextLength.HasValue)
            {
                query = query.Where(r => r.ReviewText.Length >= minTextLength.Value);
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "rating" => ascending ? query.OrderBy(r => r.Rating) : query.OrderByDescending(r => r.Rating),
                "helpful" => ascending ? query.OrderBy(r => r.HelpfulCount) : query.OrderByDescending(r => r.HelpfulCount),
                "createdat" => ascending ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt),
                _ => ascending ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Review>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<Review?> GetExistingReviewAsync(Guid accountId, Guid? orderId, Guid? productId, Guid? livestreamId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r =>
                    r.AccountID == accountId &&
                    r.OrderID == orderId &&
                    r.ProductID == productId &&
                    r.LivestreamId == livestreamId &&
                    !r.IsDeleted);
        }

        public async Task<PagedResult<Review>> GetReviewsByProductAsync(
            Guid productId, int pageNumber, int pageSize, int? minRating = null, int? maxRating = null,
            bool? verifiedOnly = null, string? sortBy = null, bool ascending = false)
        {
            return await GetByProductIdPagedAsync(productId, pageNumber, pageSize, minRating, maxRating, verifiedOnly, sortBy, ascending);
        }

        public async Task<IEnumerable<Review>> GetReviewsByOrderAsync(Guid orderId)
        {
            return await GetByOrderIdAsync(orderId);
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserAsync(Guid userId)
        {
            return await GetByUserIdAsync(userId);
        }

        public async Task<ReviewStatsResult> GetReviewStatsAsync(Guid targetId, ReviewType type)
        {
            var query = type switch
            {
                ReviewType.Product => _context.Reviews.Where(r => r.ProductID == targetId),
                ReviewType.Order => _context.Reviews.Where(r => r.OrderID == targetId),
                ReviewType.Livestream => _context.Reviews.Where(r => r.LivestreamId == targetId),
                _ => throw new ArgumentException("Invalid review type")
            };

            query = query.Where(r => !r.IsDeleted);

            var reviews = await query.ToListAsync();

            if (!reviews.Any())
            {
                return new ReviewStatsResult
                {
                    AverageRating = 0,
                    TotalReviews = 0,
                    RatingDistribution = new Dictionary<int, int>(),
                    VerifiedPurchaseCount = 0
                };
            }

            var ratingDistribution = reviews
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            // Ensure all ratings 1-5 are represented
            for (int i = 1; i <= 5; i++)
            {
                if (!ratingDistribution.ContainsKey(i))
                    ratingDistribution[i] = 0;
            }

            return new ReviewStatsResult
            {
                AverageRating = (decimal)Math.Round(reviews.Average(r => r.Rating), 2),
                TotalReviews = reviews.Count,
                RatingDistribution = ratingDistribution,
                VerifiedPurchaseCount = reviews.Count(r => r.IsVerifiedPurchase)
            };
        }

        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task InsertAsync(Review review)
        {
            await AddAsync(review);
        }

        public async Task UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var review = await GetByIdAsync(id);
            if (review != null)
            {
                review.Delete(); // ✅ FIX: Use Delete() from BaseEntity instead of SoftDelete
                await UpdateAsync(review);
            }
        }

        public async Task DeleteAsync(Guid id, string deletedBy)
        {
            var review = await GetByIdAsync(id);
            if (review != null)
            {
                review.Delete(deletedBy); // ✅ FIX: Use Delete() with modifier from BaseEntity
                await UpdateAsync(review);
            }
        }
    }

    public class ReviewStatsResult
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public int VerifiedPurchaseCount { get; set; }
    }
}