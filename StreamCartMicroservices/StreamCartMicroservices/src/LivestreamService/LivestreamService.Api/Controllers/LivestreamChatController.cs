using LivestreamService.Application.Commands.Chat;
using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Queries.Chat;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Api.Controllers
{
    [ApiController]
    [Route("api/livestream-chat")]
    [Authorize]
    public class LivestreamChatController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<LivestreamChatController> _logger;

        public LivestreamChatController(
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<LivestreamChatController> logger)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// Gửi tin nhắn trong livestream
        /// </summary>
        [HttpPost("send")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamChatDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> SendMessage([FromBody] SendLivestreamMessageDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();

                // ✅ Convert string ReplyToMessageId to Guid? for the command
                Guid? replyToMessageId = null;
                if (!string.IsNullOrEmpty(request.ReplyToMessageId) && Guid.TryParse(request.ReplyToMessageId, out var parsedGuid))
                {
                    replyToMessageId = parsedGuid;
                }

                var command = new SendLivestreamMessageCommand
                {
                    LivestreamId = request.LivestreamId,
                    SenderId = userId,
                    Message = request.Message,
                    MessageType = request.MessageType,
                    ReplyToMessageId = replyToMessageId // ✅ Now using properly converted Guid?
                };

                var result = await _mediator.Send(command);
                return Created($"/api/livestream-chat/{result.Id}",
                    ApiResponse<LivestreamChatDTO>.SuccessResult(result, "Gửi tin nhắn thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending livestream message");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy lịch sử chat của livestream
        /// </summary>
        [HttpGet("livestream/{livestreamId}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LivestreamChatDTO>>), 200)]
        public async Task<IActionResult> GetLivestreamChat(
            Guid livestreamId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] bool includeModerated = false)
        {
            try
            {
                var query = new GetLivestreamChatQuery
                {
                    LivestreamId = livestreamId,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    IncludeModerated = includeModerated
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<PagedResult<LivestreamChatDTO>>.SuccessResult(result, "Lấy lịch sử chat thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream chat");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Kiểm duyệt tin nhắn (dành cho shop owner/moderator)
        /// </summary>
        [HttpPatch("{messageId}/moderate")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> ModerateMessage(
            Guid messageId,
            [FromBody] bool isModerated)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var command = new ModerateLivestreamMessageCommand
                {
                    MessageId = messageId,
                    ModeratorId = userId,
                    IsModerated = isModerated
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<LivestreamChatDTO>.SuccessResult(result, "Kiểm duyệt tin nhắn thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating message");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
    }
}