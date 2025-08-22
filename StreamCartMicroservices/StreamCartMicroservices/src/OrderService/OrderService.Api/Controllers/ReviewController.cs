using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Queries;
using ReviewService.Domain.Enums;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using Shared.Common.Services.User;

namespace OrderService.Api.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IMediator mediator, ICurrentUserService currentUserService, ILogger<ReviewController> logger)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// ✅ UNIFIED - Tạo review cho Product/Order/Livestream
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ReviewDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));

            try
            {
                var userId = _currentUserService.GetUserId();

                // ✅ VALIDATION: Chỉ 1 trong 3 ID được có giá trị
                var idCount = new[] { request.OrderID, request.ProductID, request.LivestreamId }
                    .Count(id => id.HasValue);

                if (idCount != 1)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Phải chỉ định đúng 1 loại review (Order, Product, hoặc Livestream)"));
                }

                // ✅ AUTO-DETERMINE TYPE
                var reviewType = request.OrderID.HasValue ? ReviewType.Order
                               : request.ProductID.HasValue ? ReviewType.Product
                               : ReviewType.Livestream;

                var command = new CreateReviewCommand
                {
                    OrderID = request.OrderID,
                    ProductID = request.ProductID,
                    LivestreamId = request.LivestreamId,
                    AccountID = userId,
                    Rating = (int)request.Rating,
                    ReviewText = request.ReviewText,
                    Type = reviewType,
                    ImageUrls = request.ImageUrls,
                    IsVerifiedPurchase = await CheckVerifiedPurchase(request, userId)
                };

                var result = await _mediator.Send(command);
                return Created($"/api/reviews/{result.Id}", ApiResponse<ReviewDTO>.SuccessResult(result, "Tạo review thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ApiResponse<object>.ErrorResult(ex.Message).ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi tạo review: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy chi tiết review theo ID
        /// </summary>
        [HttpGet("{reviewId}")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetReviewById(Guid reviewId)
        {
            try
            {
                var query = new GetReviewByIdQuery { ReviewId = reviewId };
                var result = await _mediator.Send(query);

                if (result == null)
                    return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy review"));

                return Ok(ApiResponse<ReviewDTO>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review {ReviewId}", reviewId);
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy reviews của sản phẩm
        /// </summary>
        [HttpGet("products/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReviewDTO>>), 200)]
        public async Task<IActionResult> GetProductReviews(
            Guid productId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? minRating = null,
            [FromQuery] int? maxRating = null,
            [FromQuery] bool? verifiedOnly = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool ascending = false)
        {
            try
            {
                var query = new GetReviewsByProductQuery
                {
                    ProductId = productId,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    MinRating = minRating,
                    MaxRating = maxRating,
                    VerifiedPurchaseOnly = verifiedOnly,
                    SortBy = sortBy,
                    Ascending = ascending
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<PagedResult<ReviewDTO>>.SuccessResult(result, "Lấy reviews sản phẩm thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product reviews for {ProductId}", productId);
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy reviews của đơn hàng
        /// </summary>
        [HttpGet("orders/{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReviewDTO>>), 200)]
        public async Task<IActionResult> GetOrderReviews(Guid orderId)
        {
            try
            {
                var query = new GetReviewsByOrderQuery { OrderId = orderId };
                var result = await _mediator.Send(query);
                return Ok(ApiResponse<IEnumerable<ReviewDTO>>.SuccessResult(result, "Lấy reviews đơn hàng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order reviews for {OrderId}", orderId);
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy reviews của livestream
        /// </summary>
        [HttpGet("livestreams/{livestreamId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReviewDTO>>), 200)]
        public async Task<IActionResult> GetLivestreamReviews(Guid livestreamId)
        {
            try
            {
                var query = new GetReviewsByLivestreamQuery { LivestreamId = livestreamId };
                var result = await _mediator.Send(query);
                return Ok(ApiResponse<IEnumerable<ReviewDTO>>.SuccessResult(result, "Lấy reviews livestream thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream reviews for {LivestreamId}", livestreamId);
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy reviews của user
        /// </summary>
        [HttpGet("users/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReviewDTO>>), 200)]
        public async Task<IActionResult> GetUserReviews(Guid userId)
        {
            try
            {
                var currentUserId = _currentUserService.GetUserId();

                // Chỉ cho phép user xem reviews của chính mình hoặc admin
                if (currentUserId != userId && !User.IsInRole("Admin"))
                    return Forbid(ApiResponse<object>.ErrorResult("Không có quyền xem reviews của user khác").ToString());

                var query = new GetReviewsByUserQuery { UserId = userId };
                var result = await _mediator.Send(query);
                return Ok(ApiResponse<IEnumerable<ReviewDTO>>.SuccessResult(result, "Lấy reviews user thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user reviews for {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// ✅ UNIVERSAL STATS - Thống kê cho bất kỳ đối tượng nào
        /// </summary>
        [HttpGet("stats/{targetId}")]
        [ProducesResponseType(typeof(ApiResponse<ReviewStatsDTO>), 200)]
        public async Task<IActionResult> GetStats(Guid targetId, [FromQuery] string type = "Product")
        {
            try
            {
                if (!Enum.TryParse<ReviewType>(type, true, out var reviewType))
                    return BadRequest(ApiResponse<object>.ErrorResult("Loại review không hợp lệ"));

                var query = new GetReviewStatsQuery
                {
                    TargetId = targetId,
                    Type = reviewType
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<ReviewStatsDTO>.SuccessResult(result, "Lấy thống kê review thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review stats for {TargetId}", targetId);
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// ✅ ADVANCED SEARCH - Tìm kiếm nâng cao reviews
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReviewDTO>>), 200)]
        public async Task<IActionResult> SearchReviews([FromQuery] SearchReviewsDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu tìm kiếm không hợp lệ"));

            try
            {
                var query = new SearchReviewsQuery
                {
                    SearchTerm = request.SearchTerm,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    Type = request.Type,
                    TargetId = request.TargetId,
                    UserId = request.UserId,
                    MinRating = request.MinRating,
                    MaxRating = request.MaxRating,
                    VerifiedPurchaseOnly = request.VerifiedPurchaseOnly,
                    HasImages = request.HasImages,
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    SortBy = request.SortBy,
                    Ascending = request.Ascending,
                    MinHelpfulVotes = request.MinHelpfulVotes,
                    HasResponse = request.HasResponse,
                    MinTextLength = request.MinTextLength
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<PagedResult<ReviewDTO>>.SuccessResult(result, "Tìm kiếm reviews thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reviews");
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cập nhật review
        /// </summary>
        [HttpPut("{reviewId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ReviewDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateReview(Guid reviewId, [FromBody] UpdateReviewDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));

            try
            {
                var userId = _currentUserService.GetUserId();
                var command = new UpdateReviewCommand
                {
                    ReviewId = reviewId,
                    UserId = userId,
                    ReviewText = request.ReviewText,
                    Rating = request.Rating,
                    ImageUrls = request.ImageUrls
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<ReviewDTO>.SuccessResult(result, "Cập nhật review thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ApiResponse<object>.ErrorResult(ex.Message).ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", reviewId);
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Xóa review
        /// </summary>
        [HttpDelete("{reviewId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> DeleteReview(Guid reviewId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var command = new DeleteReviewCommand
                {
                    ReviewId = reviewId,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Xóa review thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ApiResponse<object>.ErrorResult(ex.Message).ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// ✅ Helper method để check verified purchase
        /// </summary>
        private async Task<bool> CheckVerifiedPurchase(CreateReviewDTO request, Guid userId)
        {
            if (request.OrderID.HasValue)
            {
                // Implement order verification logic here
                // Call OrderService to verify ownership
                return true; // Placeholder
            }
            return false; // Product và Livestream reviews không cần verified purchase
        }
    }
}