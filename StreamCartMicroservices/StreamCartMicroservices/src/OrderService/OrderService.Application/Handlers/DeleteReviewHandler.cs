using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;

namespace OrderService.Application.Handlers
{
    public class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand, bool>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ILogger<DeleteReviewHandler> _logger;

        public DeleteReviewHandler(
            IReviewRepository reviewRepository,
            ILogger<DeleteReviewHandler> logger)
        {
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting review {ReviewId} by user {UserId}",
                    request.ReviewId, request.UserId);

                var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
                if (review == null)
                {
                    throw new ArgumentException("Review không tồn tại");
                }

                // Verify ownership
                if (review.AccountID != request.UserId)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xóa review này");
                }

                // Soft delete
                review.Delete(request.UserId.ToString());
                await _reviewRepository.UpdateAsync(review);

                _logger.LogInformation("Successfully deleted review {ReviewId}", review.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", request.ReviewId);
                throw;
            }
        }
    }
}