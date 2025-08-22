using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Application.Queries;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;

namespace OrderService.Application.Handlers
{
    public class GetReviewsByProductHandler : IRequestHandler<GetReviewsByProductQuery, PagedResult<ReviewDTO>>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<GetReviewsByProductHandler> _logger;
        private readonly IAccountServiceClient _accountServiceClient;
        public GetReviewsByProductHandler(
            IReviewRepository reviewRepository,
            IProductServiceClient productServiceClient,
            IShopServiceClient shopServiceClient,
            ILogger<GetReviewsByProductHandler> logger,
            IAccountServiceClient accountServiceClient)
        {
            _reviewRepository = reviewRepository;
            _productServiceClient = productServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<PagedResult<ReviewDTO>> Handle(GetReviewsByProductQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting reviews for product {ProductId}", request.ProductId);

                // Lấy reviews từ repository
                var reviews = await _reviewRepository.GetByProductIdPagedAsync(
                    request.ProductId,
                    request.PageNumber,
                    request.PageSize,
                    request.MinRating,
                    request.MaxRating,
                    request.VerifiedPurchaseOnly,
                    request.SortBy,
                    request.Ascending);

                // Lấy thông tin sản phẩm từ Product Service
                var productInfo = await _productServiceClient.GetProductByIdAsync(request.ProductId);

                // Convert sang DTOs và enrich với thông tin từ client
                var reviewDTOs = new List<ReviewDTO>();
                foreach (var review in reviews.Items)
                {
                    var account = await _accountServiceClient.GetAccountByIdAsync(review.AccountID);
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
                        UnhelpfulCount = review.UnhelpfulCount,
                        // Enrich với thông tin từ Product Service
                        ProductName = productInfo?.ProductName,
                        ProductImageUrl = productInfo?.PrimaryImageUrl
                    };

                    // Get shop name if available
                    if (productInfo?.ShopId.HasValue == true)
                    {
                        try
                        {
                            var shopInfo = await _shopServiceClient.GetShopByIdAsync(productInfo.ShopId.Value);
                            if (shopInfo != null)
                            {
                                reviewDto.ShopName = shopInfo.ShopName;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error getting shop info for product {ProductId}", request.ProductId);
                        }
                    }

                    reviewDTOs.Add(reviewDto);
                }

                return new PagedResult<ReviewDTO>
                {
                    Items = reviewDTOs,
                    TotalCount = reviews.TotalCount,
                    CurrentPage = reviews.CurrentPage, // ✅ FIX: Use CurrentPage
                    PageSize = reviews.PageSize,
                    TotalPages = reviews.TotalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for product {ProductId}", request.ProductId);
                throw;
            }
        }
    }
}