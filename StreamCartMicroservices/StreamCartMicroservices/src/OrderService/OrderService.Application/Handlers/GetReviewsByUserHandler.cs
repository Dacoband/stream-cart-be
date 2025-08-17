using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Application.Queries;

namespace OrderService.Application.Handlers
{
    public class GetReviewsByUserHandler : IRequestHandler<GetReviewsByUserQuery, IEnumerable<ReviewDTO>>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILivestreamServiceClient _livestreamServiceClient;
        private readonly IShopServiceClient _shopServiceClient; // ✅ ADD Shop Service Client
        private readonly ILogger<GetReviewsByUserHandler> _logger;

        public GetReviewsByUserHandler(
            IReviewRepository reviewRepository,
            IProductServiceClient productServiceClient,
            ILivestreamServiceClient livestreamServiceClient,
            IShopServiceClient shopServiceClient, // ✅ ADD Shop Service Client
            ILogger<GetReviewsByUserHandler> logger)
        {
            _reviewRepository = reviewRepository;
            _productServiceClient = productServiceClient;
            _livestreamServiceClient = livestreamServiceClient;
            _shopServiceClient = shopServiceClient; // ✅ ADD Shop Service Client
            _logger = logger;
        }

        public async Task<IEnumerable<ReviewDTO>> Handle(GetReviewsByUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting reviews for user {UserId}", request.UserId);

                // Lấy reviews từ repository
                var reviews = await _reviewRepository.GetByUserIdAsync(request.UserId);

                // Convert sang DTOs và enrich với thông tin từ client
                var reviewDTOs = new List<ReviewDTO>();
                foreach (var review in reviews)
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

                                // ✅ FIX: Get shop name using ShopId from ProductDto
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

                    reviewDTOs.Add(reviewDto);
                }

                _logger.LogInformation("Successfully retrieved {Count} reviews for user {UserId}", reviewDTOs.Count, request.UserId);

                return reviewDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for user {UserId}", request.UserId);
                throw;
            }
        }
    }
}