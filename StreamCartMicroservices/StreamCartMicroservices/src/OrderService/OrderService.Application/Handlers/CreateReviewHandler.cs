using MediatR;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces.IServices;

namespace OrderService.Application.Handlers
{
    public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, ReviewDTO>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILivestreamServiceClient _livestreamServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<CreateReviewHandler> _logger;

        public CreateReviewHandler(
            IReviewRepository reviewRepository,
            IProductServiceClient productServiceClient,
            ILivestreamServiceClient livestreamServiceClient,
            IShopServiceClient shopServiceClient,
            ILogger<CreateReviewHandler> logger)
        {
            _reviewRepository = reviewRepository;
            _productServiceClient = productServiceClient;
            _livestreamServiceClient = livestreamServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
        }

        public async Task<ReviewDTO> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating review for {Type} with ID {TargetId}",
                    request.Type,
                    request.OrderID ?? request.ProductID ?? request.LivestreamId);

                // Validate that only one target ID is provided
                var idCount = new[] { request.OrderID, request.ProductID, request.LivestreamId }
                    .Count(id => id.HasValue);

                if (idCount != 1)
                {
                    throw new ArgumentException("Phải chỉ định đúng 1 loại review (Order, Product, hoặc Livestream)");
                }

                // Validate target exists and get shop info
                var shopId = await ValidateTargetAndGetShopId(request);

                // Create the review entity
                var review = new Review(
                    orderId: request.OrderID,
                    productId: request.ProductID,
                    livestreamId: request.LivestreamId,
                    accountId: request.AccountID,
                    rating: request.Rating,
                    reviewText: request.ReviewText,
                    type: request.Type,
                    isVerifiedPurchase: request.IsVerifiedPurchase,
                    imageUrls: request.ImageUrls,
                    createdBy: request.AccountID.ToString()
                );

                // Validate entity
                if (!review.IsValid())
                {
                    throw new ArgumentException("Dữ liệu review không hợp lệ");
                }

                // Save to repository
                await _reviewRepository.AddAsync(review);

                // ✅ CẬP NHẬT SHOP RATING
                if (shopId.HasValue)
                {
                    await UpdateShopRatingAsync(shopId.Value, request.Rating, request.AccountID.ToString());
                }

                // Convert to DTO manually (không dùng AutoMapper)
                var reviewDto = await ConvertToDTO(review);

                _logger.LogInformation("Successfully created review with ID {ReviewId}", review.Id);

                return reviewDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                throw;
            }
        }

        /// <summary>
        /// ✅ Validate target và lấy ShopId
        /// </summary>
        private async Task<Guid?> ValidateTargetAndGetShopId(CreateReviewCommand request)
        {
            Guid? shopId = null;

            if (request.ProductID.HasValue)
            {
                var product = await _productServiceClient.GetProductByIdAsync(request.ProductID.Value);
                if (product == null)
                {
                    throw new ArgumentException($"Sản phẩm với ID {request.ProductID} không tồn tại");
                }
                shopId = product.ShopId;
            }
            else if (request.LivestreamId.HasValue)
            {
                var livestream = await _livestreamServiceClient.GetLivestreamByIdAsync(request.LivestreamId.Value);
                if (livestream == null)
                {
                    throw new ArgumentException($"Livestream với ID {request.LivestreamId} không tồn tại");
                }
                // Có thể lấy ShopId từ livestream nếu có
                // shopId = livestream.ShopId;
            }

            return shopId;
        }

        /// <summary>
        /// ✅ Cập nhật rating cho shop
        /// </summary>
        private async Task UpdateShopRatingAsync(Guid shopId, int newRating, string modifier)
        {
            try
            {
                var success = await _shopServiceClient.UpdateShopRatingAsync(shopId, newRating, modifier);

                if (success)
                {
                    _logger.LogInformation("✅ Successfully updated shop {ShopId} rating with new rating: {Rating}",
                        shopId, newRating);
                }
                else
                {
                    _logger.LogWarning("⚠️ Failed to update shop {ShopId} rating", shopId);
                }
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng đến việc tạo review
                _logger.LogError(ex, "❌ Error updating shop {ShopId} rating", shopId);
            }
        }

        private async Task<ReviewDTO> ConvertToDTO(Review review)
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
                var productInfo = await _productServiceClient.GetProductByIdAsync(review.ProductID.Value);
                if (productInfo != null)
                {
                    reviewDto.ProductName = productInfo.ProductName;
                    reviewDto.ProductImageUrl = productInfo.PrimaryImageUrl;

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

            if (review.LivestreamId.HasValue)
            {
                var livestreamInfo = await _livestreamServiceClient.GetLivestreamBasicInfoAsync(review.LivestreamId.Value);
                if (livestreamInfo != null)
                {
                    reviewDto.LivestreamTitle = livestreamInfo.Title;
                    reviewDto.ShopName = livestreamInfo.ShopName;
                }
            }

            return reviewDto;
        }
    }
}