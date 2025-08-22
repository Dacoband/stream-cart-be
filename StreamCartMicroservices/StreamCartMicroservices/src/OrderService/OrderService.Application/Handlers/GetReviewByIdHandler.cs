using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Application.Queries;

namespace OrderService.Application.Handlers
{
    public class GetReviewByIdHandler : IRequestHandler<GetReviewByIdQuery, ReviewDTO?>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILivestreamServiceClient _livestreamServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<GetReviewByIdHandler> _logger;
        private readonly IAccountServiceClient _accountServiceClient;

        public GetReviewByIdHandler(
            IReviewRepository reviewRepository,
            IProductServiceClient productServiceClient,
            ILivestreamServiceClient livestreamServiceClient,
            IShopServiceClient shopServiceClient,
            ILogger<GetReviewByIdHandler> logger,
            IAccountServiceClient accountServiceClient)
        {
            _reviewRepository = reviewRepository;
            _productServiceClient = productServiceClient;
            _livestreamServiceClient = livestreamServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<ReviewDTO?> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting review by ID {ReviewId}", request.ReviewId);

                var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
                if (review == null || review.IsDeleted)
                {
                    _logger.LogWarning("Review {ReviewId} not found or deleted", request.ReviewId);
                    return null;
                }
                var account = await _accountServiceClient.GetAccountByIdAsync(review.AccountID);
                // Convert to DTO manually và enrich với thông tin từ clients
                var reviewDto = new ReviewDTO
                {
                    Id = review.Id,
                    OrderID = review.OrderID,
                    ProductID = review.ProductID,
                    LivestreamId = review.LivestreamId,
                    AccountID = review.AccountID,
                    UserName = account?.FullName,
                    AvatarImage = account?.AvatarURL,
                    Rating = review.Rating,
                    ReviewText = review.ReviewText,
                    IsVerifiedPurchase = review.IsVerifiedPurchase,
                    Type = review.Type,
                    ImageUrls = review.ImageUrls,
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

                            // Get shop name using ShopId
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

                _logger.LogInformation("Successfully retrieved review {ReviewId}", request.ReviewId);

                return reviewDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review {ReviewId}", request.ReviewId);
                throw;
            }
        }
    }
}