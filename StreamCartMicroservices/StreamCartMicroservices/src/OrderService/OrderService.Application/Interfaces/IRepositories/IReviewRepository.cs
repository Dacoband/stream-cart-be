using OrderService.Domain.Entities;
using ReviewService.Domain.Enums;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;

namespace OrderService.Application.Interfaces.IRepositories
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(Guid id);
        Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<Review>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Review>> GetByLivestreamIdAsync(Guid livestreamId);
        Task<IEnumerable<Review>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Review>> GetByTargetAsync(Guid targetId, ReviewType type);
        Task<PagedResult<Review>> GetByProductIdPagedAsync(
            Guid productId,
            int pageNumber,
            int pageSize,
            int? minRating = null,
            int? maxRating = null,
            bool? verifiedPurchaseOnly = null,
            string? sortBy = null,
            bool ascending = false);
        Task<PagedResult<Review>> SearchAsync(
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
            int? minTextLength = null);
        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(Guid id);
    }
}