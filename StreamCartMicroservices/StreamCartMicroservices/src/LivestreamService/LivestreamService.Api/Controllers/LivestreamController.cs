using Livestreamservice.Application.Commands;
using Livestreamservice.Application.DTOs;
using Livestreamservice.Application.Queries;
using LivestreamService.Application.Commands;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LivestreamService.Api.Controllers
{
    [ApiController]
    [Route("api/livestreams")]
    public class LivestreamController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILivekitService _livekitService;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<LivestreamController> _logger;

        public LivestreamController(
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILivekitService livekitService,
            ILivestreamRepository livestreamRepository,
            IShopServiceClient shopServiceClient,
            ILogger<LivestreamController> logger)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _livekitService = livekitService;
            _livestreamRepository = livestreamRepository;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
        }

        /// <summary>
        /// Create a new livestream (Seller only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateLivestream([FromBody] CreateLivestreamDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid input data"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();
                var shopId = _currentUserService.GetShopId(); 

                var command = new CreateLivestreamCommand
                {
                    Title = request.Title,
                    Description = request.Description,
                    ShopId =Guid.Parse(shopId),
                    LivestreamHostId = request.LivestreamHostId,
                    ScheduledStartTime = request.ScheduledStartTime,
                    ThumbnailUrl = request.ThumbnailUrl,
                    Tags = request.Tags,
                    SellerId = userId,
                    Products = request.Products
                };

                var result = await _mediator.Send(command);
                return Created($"/api/livestreams/{result.Id}", ApiResponse<LivestreamDTO>.SuccessResult(result, "Livestream created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error creating livestream: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a livestream by ID
        /// </summary>
        [HttpGet("{id}")]
       // [Authorize]
        [ProducesResponseType(typeof(ApiResponse<LivestreamDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetLivestream(Guid id)
        {
            try
            {
                var query = new GetLivestreamByIdQuery { Id = id };
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Livestream not found"));
                }

                return Ok(ApiResponse<LivestreamDTO>.SuccessResult(result, "Livestream retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving livestream with ID {LivestreamId}", id);
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving livestream: {ex.Message}"));
            }
        }

        /// <summary>
        /// Join a livestream (authenticated users)
        /// </summary>
        [HttpGet("{id}/join")]
       // [Authorize]
        [ProducesResponseType(typeof(ApiResponse<LivestreamDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> JoinLivestream(Guid id)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var isCustomer = User.IsInRole("Customer");
                var isSeller = User.IsInRole("Seller");

                // Fetch the livestream
                var livestream = await _livestreamRepository.GetByIdAsync(id.ToString());
                if (livestream == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Livestream not found"));
                }

                // Check if user is the owner of the livestream
                bool isOwner = livestream.SellerId == userId;

                // Generate join token with appropriate permissions
                var token = await _livekitService.GenerateJoinTokenAsync(
                    livestream.LivekitRoomId,
                    userId.ToString(),
                    isOwner || isSeller // Only sellers can publish
                );

                // Get shop information
                var shop = await _shopServiceClient.GetShopByIdAsync(livestream.ShopId);

                var result = new LivestreamDTO
                {
                    Id = livestream.Id,
                    Title = livestream.Title,
                    Description = livestream.Description,
                    SellerId = livestream.SellerId,
                    ShopId = livestream.ShopId,
                    ShopName = shop?.ShopName,
                    ScheduledStartTime = livestream.ScheduledStartTime,
                    ActualStartTime = livestream.ActualStartTime,
                    ActualEndTime = livestream.ActualEndTime,
                    Status = livestream.Status,
                    StreamKey = livestream.StreamKey,
                    PlaybackUrl = livestream.PlaybackUrl,
                    LivekitRoomId = livestream.LivekitRoomId,
                    JoinToken = token,
                    ThumbnailUrl = livestream.ThumbnailUrl,
                    Tags = livestream.Tags
                };

                return Ok(ApiResponse<LivestreamDTO>.SuccessResult(result, "Join token generated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error joining livestream: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get active livestreams (authenticated users)
        /// </summary>
        [HttpGet("active")]
       // [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<LivestreamDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetActiveLivestreams([FromQuery] bool promotedOnly = false)
        {
            try
            {
                var query = new GetActiveLivestreamsQuery { IncludePromotedOnly = promotedOnly };
                var result = await _mediator.Send(query);

                return Ok(ApiResponse<List<LivestreamDTO>>.SuccessResult(result, "Active livestreams retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active livestreams");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving active livestreams: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get upcoming livestreams
        /// </summary>
        [HttpGet("upcoming")]
        //[Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<LivestreamDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetUpcomingLivestreams([FromQuery] bool promotedOnly = false)
        {
            try
            {
                var query = new GetUpcomingLivestreamsQuery { IncludePromotedOnly = promotedOnly };
                var result = await _mediator.Send(query);

                return Ok(ApiResponse<List<LivestreamDTO>>.SuccessResult(result, "Upcoming livestreams retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming livestreams");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving upcoming livestreams: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get seller's livestreams
        /// </summary>
        [HttpGet("seller/{sellerId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<LivestreamDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetSellerLivestreams(Guid sellerId)
        {
            try
            {
                var query = new GetSellerLivestreamsQuery { SellerId = sellerId };
                var result = await _mediator.Send(query);

                return Ok(ApiResponse<List<LivestreamDTO>>.SuccessResult(result, "Seller livestreams retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seller livestreams");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving seller livestreams: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get shop's livestreams
        /// </summary>
        [HttpGet("shop/{shopId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<LivestreamDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetShopLivestreams(Guid shopId)
        {
            try
            {
                var query = new GetShopLivestreamsQuery { ShopId = shopId };
                var result = await _mediator.Send(query);

                return Ok(ApiResponse<List<LivestreamDTO>>.SuccessResult(result, "Shop livestreams retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shop livestreams");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving shop livestreams: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update a livestream (Seller only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateLivestream(Guid id, [FromBody] UpdateLivestreamDTO request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                // Verify ownership
                var livestream = await _livestreamRepository.GetByIdAsync(id.ToString());
                if (livestream == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Livestream not found"));
                }

                if (livestream.SellerId != userId)
                {
                    return Forbid();
                }

                var command = new UpdateLivestreamCommand
                {
                    Id = id,
                    Title = request.Title,
                    Description = request.Description,
                    ScheduledStartTime = request.ScheduledStartTime,
                    ThumbnailUrl = request.ThumbnailUrl,
                    Tags = request.Tags,
                    UpdatedBy = userId.ToString()
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<LivestreamDTO>.SuccessResult(result, "Livestream updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating livestream: {ex.Message}"));
            }
        }

        /// <summary>
        /// Start a livestream (Seller only)
        /// </summary>
        [HttpPost("{id}/start")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> StartLivestream(Guid id)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new StartLivestreamCommand
                {
                    Id = id,
                    SellerId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<LivestreamDTO>.SuccessResult(result, "Livestream started successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error starting livestream: {ex.Message}"));
            }
        }

        /// <summary>
        /// End a livestream (Seller only)
        /// </summary>
        [HttpPost("{id}/end")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> EndLivestream(Guid id)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new EndLivestreamCommand
                {
                    Id = id,
                    SellerId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<LivestreamDTO>.SuccessResult(result, "Livestream ended successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error ending livestream: {ex.Message}"));
            }
        }

        /// <summary>
        /// Set promotion status for a livestream (Admin only)
        /// </summary>
        [HttpPost("{id}/promote")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> PromoteLivestream(Guid id, [FromBody] PromoteLivestreamDTO request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new PromoteLivestreamCommand
                {
                    Id = id,
                    IsPromoted = request.IsPromoted,
                    AdminId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<LivestreamDTO>.SuccessResult(result,
                    request.IsPromoted ? "Livestream promoted successfully" : "Livestream promotion removed successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting promotion status for livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error setting promotion status: {ex.Message}"));
            }
        }

        /// <summary>
        /// Approve or reject livestream content (Admin or Moderator only)
        /// </summary>
        [HttpPost("{id}/approve-content")]
        [Authorize(Roles = "Admin,Moderator")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> ApproveLivestreamContent(Guid id, [FromBody] ApproveLivestreamContentDTO request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new ApproveLivestreamContentCommand
                {
                    Id = id,
                    Approved = request.Approved,
                    ModeratorId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<LivestreamDTO>.SuccessResult(result,
                    request.Approved ? "Livestream content approved successfully" : "Livestream content not approved"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving livestream content");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error approving content: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a livestream (Seller who owns it or Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> DeleteLivestream(Guid id)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var isAdmin = User.IsInRole("Admin");

                // Fetch the livestream
                var livestream = await _livestreamRepository.GetByIdAsync(id.ToString());
                if (livestream == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Livestream not found"));
                }

                // Check if user has permission to delete
                if (!isAdmin && livestream.SellerId != userId)
                {
                    return Forbid();
                }

                // Delete the livestream
                await _livestreamRepository.DeleteAsync(id.ToString());

                // Attempt to delete the LiveKit room
                await _livekitService.DeleteRoomAsync(livestream.LivekitRoomId);

                return Ok(ApiResponse<object>.SuccessResult(null, "Livestream deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting livestream: {ex.Message}"));
            }
        }
        /// <summary>
        /// Get livestream statistics for a shop
        /// </summary>
        [HttpGet("shop/{shopId}/statistics")]
        [Authorize(Roles = "Seller,Admin,OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamStatisticsDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetLivestreamStatistics(
            Guid shopId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                // Set default date range if not provided
                var from = fromDate ?? DateTime.UtcNow.Date.AddDays(-30);
                var to = toDate ?? DateTime.UtcNow;

                // Get all livestreams for the shop within the date range
                var livestreams = await _livestreamRepository.GetLivestreamsByShopIdAsync(shopId);

                // Filter by date range
                var filteredLivestreams = livestreams.Where(l =>
                    (l.ScheduledStartTime >= from && l.ScheduledStartTime <= to) ||
                    (l.ActualStartTime.HasValue && l.ActualStartTime.Value >= from && l.ActualStartTime.Value <= to)
                ).ToList();

                // Calculate statistics
                var totalLivestreams = filteredLivestreams.Count;

                // Calculate total duration in minutes
                decimal totalDuration = 0;
                foreach (var livestream in filteredLivestreams)
                {
                    if (livestream.ActualStartTime.HasValue)
                    {
                        var endTime = livestream.ActualEndTime ?? to; // Use current time if still ongoing
                        var duration = (decimal)(endTime - livestream.ActualStartTime.Value).TotalMinutes;
                        totalDuration += Math.Max(0, duration); // Ensure non-negative
                    }
                }

                // Sum viewer counts
                var totalViewers = filteredLivestreams.Sum(l => l.MaxViewer ?? 0);

                // Create statistics DTO
                var statistics = new LivestreamStatisticsDTO
                {
                    TotalLivestreams = totalLivestreams,
                    TotalDuration = totalDuration,
                    TotalViewers = totalViewers,
                    FromDate = from,
                    ToDate = to
                };

                return Ok(ApiResponse<LivestreamStatisticsDTO>.SuccessResult(statistics, "Livestream statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving livestream statistics for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving livestream statistics: {ex.Message}"));
            }
        }
    }
}