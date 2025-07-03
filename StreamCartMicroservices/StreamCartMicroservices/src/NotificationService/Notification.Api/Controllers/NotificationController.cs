 using MassTransit.Futures.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Command;
using Notification.Application.DTOs;
using Notification.Application.Queries;
using Shared.Common.Models;
using System.Security.Claims;

namespace Notification.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator _mediator;
        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ListNotificationDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetMyNotification([FromQuery] FilterNotificationDTO filter)
        {

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "123";
                var command = new GetMyNotificationQuery
                {
                    FilterNotificationDTO = filter,
                    UserId = userId,
                };
                var apiResponse =  await _mediator.Send(command);
                if (apiResponse.Success == true) { return Ok(apiResponse); }
                else return BadRequest(apiResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tìm thông báo: {ex.Message}"));
            }
        }
        [HttpPatch("/mark-as-read/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
        public async Task<IActionResult> MarkAsRead([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "123";
                var command = new MarkAsRead
                {
                   Id = id,
                };
                var apiResponse = await _mediator.Send(command);
                if (apiResponse.Success == true) { return Ok(apiResponse); }
                else return BadRequest(apiResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi đánh dấu thông báo: {ex.Message}"));
            }
        }
 
    }
}
