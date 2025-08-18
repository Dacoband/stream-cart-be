using MediatR;
using OrderService.Application.Queries;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace OrderService.Application.Handlers
{
    public class GetReviewsByLivestreamHandler : IRequestHandler<GetReviewsByLivestreamQuery, IEnumerable<ReviewDTO>>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ILivestreamServiceClient _livestreamServiceClient;
        private readonly ILogger<GetReviewsByLivestreamHandler> _logger;
        private readonly IAccountServiceClient _accountServiceClient;

        public GetReviewsByLivestreamHandler(
            IReviewRepository reviewRepository,
            ILivestreamServiceClient livestreamServiceClient,
            ILogger<GetReviewsByLivestreamHandler> logger,
            IAccountServiceClient accountServiceClient)
        {
            _reviewRepository = reviewRepository;
            _livestreamServiceClient = livestreamServiceClient;
            _logger = logger;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<IEnumerable<ReviewDTO>> Handle(GetReviewsByLivestreamQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting reviews for livestream {LivestreamId}", request.LivestreamId);

                // Lấy reviews từ repository
                var reviews = await _reviewRepository.GetByLivestreamIdAsync(request.LivestreamId);

                // Lấy thông tin livestream từ Livestream Service
                var livestreamInfo = await _livestreamServiceClient.GetLivestreamBasicInfoAsync(request.LivestreamId);
                // Convert sang DTOs và enrich với thông tin từ client
                var reviewDTOs = new List<ReviewDTO>();
                foreach (var review in reviews)
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
                        // Enrich với thông tin từ Livestream Service
                        LivestreamTitle = livestreamInfo?.Title,
                        ShopName = livestreamInfo?.ShopName
                    };

                    reviewDTOs.Add(reviewDto);
                }

                return reviewDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}