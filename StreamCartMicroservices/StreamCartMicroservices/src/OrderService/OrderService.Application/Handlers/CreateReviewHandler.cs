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

                // Validate target exists
                await ValidateTargetExists(request);

               string? reviewImageUrl = null;

                if (request.ProductID.HasValue)
                {
                    // For product reviews, automatically get product image
                    var product  = await _productServiceClient.GetProductByIdAsync(request.ProductID.Value);
                    reviewImageUrl = product.PrimaryImageUrl ?? string.Empty;
                }
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
                    imageUrl: reviewImageUrl ,
                    createdBy: request.AccountID.ToString()
                );

                // Validate entity
                if (!review.IsValid())
                {
                    throw new ArgumentException("Dữ liệu review không hợp lệ");
                }

                // Save to repository
                await _reviewRepository.AddAsync(review);

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

        private async Task ValidateTargetExists(CreateReviewCommand request)
        {
            if (request.ProductID.HasValue)
            {
                // ✅ FIX: Sử dụng GetProductByIdAsync thay vì DoesProductExistAsync
                var product = await _productServiceClient.GetProductByIdAsync(request.ProductID.Value);
                if (product == null)
                {
                    throw new ArgumentException($"Sản phẩm với ID {request.ProductID} không tồn tại");
                }
            }
            else if (request.LivestreamId.HasValue)
            {
                var livestream = await _livestreamServiceClient.GetLivestreamByIdAsync(request.LivestreamId.Value);
                if (livestream == null)
                {
                    throw new ArgumentException($"Livestream với ID {request.LivestreamId} không tồn tại");
                }
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
                var productInfo = await _productServiceClient.GetProductByIdAsync(review.ProductID.Value);
                if (productInfo != null)
                {
                    reviewDto.ProductName = productInfo.ProductName;
                    reviewDto.ProductImageUrl = productInfo.PrimaryImageUrl;

                    // ✅ FIX: Lấy shop name thông qua ShopId từ ProductDto
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