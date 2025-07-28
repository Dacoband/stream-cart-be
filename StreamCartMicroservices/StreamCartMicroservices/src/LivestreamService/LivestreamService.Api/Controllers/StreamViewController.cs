using LivestreamService.Application.Commands.StreamView;
using LivestreamService.Application.DTOs.StreamView;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries.StreamView;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LivestreamService.Api.Controllers
{
    [ApiController]
    [Route("api/stream-views")]
    public class StreamViewController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStreamViewRepository _streamViewRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly ILogger<StreamViewController> _logger;

        public StreamViewController(
            IMediator mediator,
            ICurrentUserService currentUserService,
            IStreamViewRepository streamViewRepository,
            IAccountServiceClient accountServiceClient,
            ILogger<StreamViewController> logger)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _streamViewRepository = streamViewRepository;
            _accountServiceClient = accountServiceClient;
            _logger = logger;
        }

        /// <summary>
        /// Bắt đầu xem livestream
        /// </summary>
        [HttpPost("start")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<StreamViewDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> StartStreamView([FromBody] StartStreamViewDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new StartStreamViewCommand
                {
                    LivestreamId = request.LivestreamId,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Created($"/api/stream-views/{result.Id}",
                    ApiResponse<StreamViewDTO>.SuccessResult(result, "Bắt đầu xem livestream thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi bắt đầu xem livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Kết thúc xem livestream
        /// </summary>
        [HttpPost("end")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<StreamViewDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> EndStreamView([FromBody] EndStreamViewDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new EndStreamViewCommand
                {
                    StreamViewId = request.StreamViewId,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<StreamViewDTO>.SuccessResult(result, "Kết thúc xem livestream thành công"));
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
                _logger.LogError(ex, "Lỗi khi kết thúc xem livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thống kê lượt xem của livestream
        /// </summary>
        [HttpGet("livestream/{livestreamId}/stats")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<StreamViewStatsDTO>), 200)]
        public async Task<IActionResult> GetStreamViewStats(Guid livestreamId)
        {
            try
            {
                var query = new GetStreamViewStatsQuery { LivestreamId = livestreamId };
                var result = await _mediator.Send(query);

                return Ok(ApiResponse<StreamViewStatsDTO>.SuccessResult(result, "Lấy thống kê lượt xem thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê lượt xem");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách người xem hiện tại
        /// </summary>
        [HttpGet("livestream/{livestreamId}/viewers")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<StreamViewDTO>>), 200)]
        public async Task<IActionResult> GetCurrentViewers(Guid livestreamId)
        {
            try
            {
                var views = await _streamViewRepository.GetByLivestreamIdAsync(livestreamId);
                var activeViews = views.Where(v => v.EndTime == null).ToList();

                var result = new List<StreamViewDTO>();
                foreach (var view in activeViews)
                {
                    var user = await _accountServiceClient.GetAccountByIdAsync(view.UserId);
                    result.Add(new StreamViewDTO
                    {
                        Id = view.Id,
                        LivestreamId = view.LivestreamId,
                        UserId = view.UserId,
                        StartTime = view.StartTime,
                        EndTime = view.EndTime,
                        IsActive = view.EndTime == null,
                        CreatedAt = view.CreatedAt,
                        UserName = user?.Username
                    });
                }

                return Ok(ApiResponse<IEnumerable<StreamViewDTO>>.SuccessResult(result, "Lấy danh sách người xem thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách người xem");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy lịch sử xem của user
        /// </summary>
        [HttpGet("user/{userId}/history")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<StreamViewDTO>>), 200)]
        public async Task<IActionResult> GetUserViewHistory(Guid userId)
        {
            try
            {
                var currentUserId = _currentUserService.GetUserId();
                var isAdmin = User.IsInRole("Admin");

                // Users can only see their own history unless they're admin
                if (userId != currentUserId && !isAdmin)
                {
                    return Forbid();
                }

                var views = await _streamViewRepository.GetByUserIdAsync(userId);
                var user = await _accountServiceClient.GetAccountByIdAsync(userId);

                var result = views.Select(v => new StreamViewDTO
                {
                    Id = v.Id,
                    LivestreamId = v.LivestreamId,
                    UserId = v.UserId,
                    StartTime = v.StartTime,
                    EndTime = v.EndTime,
                    Duration = v.EndTime.HasValue ? v.EndTime.Value - v.StartTime : null,
                    IsActive = v.EndTime == null,
                    CreatedAt = v.CreatedAt,
                    UserName = user?.Username
                });

                return Ok(ApiResponse<IEnumerable<StreamViewDTO>>.SuccessResult(result, "Lấy lịch sử xem thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử xem của user");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
    }
}