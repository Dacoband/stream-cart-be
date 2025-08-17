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
    public class SearchReviewsHandler : IRequestHandler<SearchReviewsQuery, PagedResult<ReviewDTO>>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILivestreamServiceClient _livestreamServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<SearchReviewsHandler> _logger;

        public SearchReviewsHandler(
            IReviewRepository reviewRepository,
            IProductServiceClient productServiceClient,
            ILivestreamServiceClient livestreamServiceClient,
            IShopServiceClient shopServiceClient,
            ILogger<SearchReviewsHandler> logger)
        {
            _reviewRepository = reviewRepository;
            _productServiceClient = productServiceClient;
            _livestreamServiceClient = livestreamServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
        }

        public async Task<PagedResult<ReviewDTO>> Handle(SearchReviewsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Searching reviews with term: {SearchTerm}", request.SearchTerm);

                var result = await _reviewRepository.SearchAsync(
                    searchTerm: request.SearchTerm,
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    type: request.Type,
                    targetId: request.TargetId,
                    userId: request.UserId,
                    minRating: request.MinRating,
                    maxRating: request.MaxRating,
                    verifiedPurchaseOnly: request.VerifiedPurchaseOnly,
                    hasImages: request.HasImages,
                    fromDate: request.FromDate,
                    toDate: request.ToDate,
                    sortBy: request.SortBy,
                    ascending: request.Ascending,
                    minHelpfulVotes: request.MinHelpfulVotes,
                    hasResponse: request.HasResponse,
                    minTextLength: request.MinTextLength
                );

                // Convert to DTOs manually và enrich với thông tin từ clients
                var reviewDTOs = new List<ReviewDTO>();
                foreach (var review in result.Items)
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

                    reviewDTOs.Add(reviewDto);
                }

                return new PagedResult<ReviewDTO>
                {
                    Items = reviewDTOs,
                    TotalCount = result.TotalCount,
                    CurrentPage = result.CurrentPage, 
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reviews");
                throw;
            }
        }
    }
}