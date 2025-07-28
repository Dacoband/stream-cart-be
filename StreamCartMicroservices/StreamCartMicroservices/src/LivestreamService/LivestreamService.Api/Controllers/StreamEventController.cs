using LivestreamService.Application.Commands.StreamEvent;
using LivestreamService.Application.DTOs.StreamEvent;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries.StreamEvent;
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
    [Route("api/stream-events")]
    public class StreamEventController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStreamEventRepository _streamEventRepository;
        private readonly ILogger<StreamEventController> _logger;

        public StreamEventController(
            IMediator mediator,
            ICurrentUserService currentUserService,
            IStreamEventRepository streamEventRepository,
            ILogger<StreamEventController> logger)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _streamEventRepository = streamEventRepository;
            _logger = logger;
        }

        /// <summary>
        /// Tạo event mới trong livestream
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<StreamEventDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateStreamEvent([FromBody] CreateStreamEventDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new CreateStreamEventCommand
                {
                    LivestreamId = request.LivestreamId,
                    UserId = userId,
                    LivestreamProductId = request.LivestreamProductId,
                    EventType = request.EventType,
                    Payload = request.Payload
                };

                var result = await _mediator.Send(command);
                return Created($"/api/stream-events/{result.Id}",
                    ApiResponse<StreamEventDTO>.SuccessResult(result, "Tạo stream event thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo stream event");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách events của livestream
        /// </summary>
        [HttpGet("livestream/{livestreamId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<StreamEventDTO>>), 200)]
        public async Task<IActionResult> GetStreamEventsByLivestream(
            Guid livestreamId,
            [FromQuery] int? count = null,
            [FromQuery] string? eventType = null)
        {
            try
            {
                var query = new GetStreamEventsByLivestreamQuery
                {
                    LivestreamId = livestreamId,
                    Count = count,
                    EventType = eventType
                };
                var result = await _mediator.Send(query);

                return Ok(ApiResponse<IEnumerable<StreamEventDTO>>.SuccessResult(result, "Lấy danh sách events thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách stream events");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy events gần đây của livestream
        /// </summary>
        [HttpGet("livestream/{livestreamId}/recent")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<StreamEventDTO>>), 200)]
        public async Task<IActionResult> GetRecentStreamEvents(Guid livestreamId, [FromQuery] int count = 50)
        {
            try
            {
                var events = await _streamEventRepository.GetRecentEventsByLivestreamAsync(livestreamId, count);
                var result = events.Select(e => new StreamEventDTO
                {
                    Id = e.Id,
                    LivestreamId = e.LivestreamId,
                    UserId = e.UserId,
                    LivestreamProductId = e.LivestreamProductId,
                    EventType = e.EventType,
                    Payload = e.Payload,
                    CreatedAt = e.CreatedAt,
                    CreatedBy = e.CreatedBy
                });

                return Ok(ApiResponse<IEnumerable<StreamEventDTO>>.SuccessResult(result, "Lấy events gần đây thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy events gần đây");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy events của user
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<StreamEventDTO>>), 200)]
        public async Task<IActionResult> GetStreamEventsByUser(Guid userId)
        {
            try
            {
                var currentUserId = _currentUserService.GetUserId();
                var isAdmin = User.IsInRole("Admin");

                // Users can only see their own events unless they're admin
                if (userId != currentUserId && !isAdmin)
                {
                    return Forbid();
                }

                var events = await _streamEventRepository.GetByUserIdAsync(userId);
                var result = events.Select(e => new StreamEventDTO
                {
                    Id = e.Id,
                    LivestreamId = e.LivestreamId,
                    UserId = e.UserId,
                    LivestreamProductId = e.LivestreamProductId,
                    EventType = e.EventType,
                    Payload = e.Payload,
                    CreatedAt = e.CreatedAt,
                    CreatedBy = e.CreatedBy
                });

                return Ok(ApiResponse<IEnumerable<StreamEventDTO>>.SuccessResult(result, "Lấy events của user thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy events của user");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Xóa event (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> DeleteStreamEvent(Guid id)
        {
            try
            {
                await _streamEventRepository.DeleteAsync(id.ToString());
                return Ok(ApiResponse<bool>.SuccessResult(true, "Xóa stream event thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa stream event");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
    }
}