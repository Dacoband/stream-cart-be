using MediatR;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces.IServices;

namespace OrderService.Application.Handlers
{
    public class UpdateReviewHandler : IRequestHandler<UpdateReviewCommand, ReviewDTO>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILivestreamServiceClient _livestreamServiceClient;
        private readonly IShopServiceClient _shopServiceClient; // ✅ ADD Shop Service Client
        private readonly ILogger<UpdateReviewHandler> _logger;

        public UpdateReviewHandler(
            IReviewRepository reviewRepository,
            IProductServiceClient productServiceClient,
            ILivestreamServiceClient livestreamServiceClient,
            IShopServiceClient shopServiceClient, // ✅ ADD Shop Service Client
            ILogger<UpdateReviewHandler> logger)
        {
            _reviewRepository = reviewRepository;
            _productServiceClient = productServiceClient;
            _livestreamServiceClient = livestreamServiceClient;
            _shopServiceClient = shopServiceClient; // ✅ ADD Shop Service Client
            _logger = logger;
        }

        public async Task<ReviewDTO> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating review {ReviewId} by user {UserId}",
                    request.ReviewId, request.UserId);

                var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
                if (review == null)
                {
                    throw new ArgumentException("Review không tồn tại");
                }

                // Verify ownership
                if (review.AccountID != request.UserId)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền cập nhật review này");
                }

                // Update review
                review.UpdateReview(
                    reviewText: request.ReviewText,
                    rating: request.Rating,
                    imageUrl: review.ImageUrl,
                    modifiedBy: request.UserId.ToString()
                );

                // Save changes
                await _reviewRepository.UpdateAsync(review);

                // Convert to DTO manually
                var reviewDto = await ConvertToDTO(review);

                _logger.LogInformation("Successfully updated review {ReviewId}", review.Id);

                return reviewDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", request.ReviewId);
                throw;
            }
        }

        private async Task<ReviewDTO> ConvertToDTO(Domain.Entities.Review review)
        {
            var reviewDto = new ReviewDTO
            {
                Id = review.Id,
                OrderID = review.OrderID,
                ProductID = review.ProductID,
                LivestreamId = review.LivestreamId,
                AccountID = review.AccountID,
                Rating = review.Rating,
                ReviewText = review.ReviewText,
                IsVerifiedPurchase = review.IsVerifiedPurchase,
                Type = review.Type,
                ImageUrl = review.ImageUrl,
                CreatedAt = review.CreatedAt,
                ApprovedAt = review.ApprovedAt,
                ApprovedBy = review.ApprovedBy,
                HelpfulCount = review.HelpfulCount,
                UnhelpfulCount = review.UnhelpfulCount
            };

            // Enrich với thông tin từ các service clients
            if (review.ProductID.HasValue)
            {
                try
                {
                    var productInfo = await _productServiceClient.GetProductByIdAsync(review.ProductID.Value);
                    if (productInfo != null)
                    {
                        reviewDto.ProductName = productInfo.ProductName;
                        reviewDto.ProductImageUrl = productInfo.PrimaryImageUrl;

                        // ✅ GET SHOP NAME using ShopId from ProductDto
                        if (productInfo.ShopId.HasValue)
                        {
                            var shopInfo = await _shopServiceClient.GetShopByIdAsync(productInfo.ShopId.Value);
                            if (shopInfo != null)
                            {
                                reviewDto.ShopName = shopInfo.ShopName;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting product/shop info for review {ReviewId}", review.Id);
                }
            }

            if (review.LivestreamId.HasValue)
            {
                try
                {
                    var livestreamInfo = await _livestreamServiceClient.GetLivestreamBasicInfoAsync(review.LivestreamId.Value);
                    if (livestreamInfo != null)
                    {
                        reviewDto.LivestreamTitle = livestreamInfo.Title;
                        reviewDto.ShopName = livestreamInfo.ShopName;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting livestream info for review {ReviewId}", review.Id);
                }
            }

            return reviewDto;
        }
    }
}