using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Models;
using ShopService.Application.Commands;
using ShopService.Application.DTOs.Membership;
using ShopService.Application.Handlers.MembershipHandler;
using ShopService.Application.Queries;
using ShopService.Domain.Entities;

namespace ShopService.Api.Controllers
{
    [ApiController]
    [Route("api/membership")]
    public class MembershipController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MembershipController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]
        [Authorize(Roles = "OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<DetailMembershipDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateMembership([FromBody] CreateMembershipDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                string userId = User.FindFirst("id")?.Value;
                CreateMembershipCommand command = new CreateMembershipCommand()
                {
                   command = request,
                   userId = userId,
                };

                var apiResponse = await _mediator.Send(command);
                if (apiResponse.Success == true)
                {
                    return Created(userId, apiResponse);
                }
                else return BadRequest(apiResponse);
            }
            catch (Exception ex)
            {

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi cập nhật gói thành viên: {ex.Message}"));
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<DetailMembershipDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateMembership([FromBody] UpdateMembershipDTO request, [FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                string userId = User.FindFirst("id")?.Value;

                var command = new UpdateMembershipCommand()
                {
                    command = request,
                    MembershipId = id,
                    UserId = userId,
                };
                var apiResponse = await _mediator.Send(command);
                if (apiResponse.Success == true)
                {
                    return Ok(apiResponse);
                }
                else return BadRequest(apiResponse);
            }
            catch (Exception ex)
            {

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tạo mới gói thành viên: {ex.Message}"));
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<DetailMembershipDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> DeleteMembership([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                string userId = User.FindFirst("id")?.Value;

                var command = new DeleteMemershipCommand
                {
                    
                    MembershipId = id,
                    UserId = userId,
                };
                var apiResponse = await _mediator.Send(command);
                if (apiResponse.Success == true)
                {
                    return Ok(apiResponse);
                }
                else return BadRequest(apiResponse);
            }
            catch (Exception ex)
            {

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi xóa gói thành viên: {ex.Message}"));
            }
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ListMembershipDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> FilterMembership([FromQuery] FilterMembershipDTO filter)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                var command = new FilterMembershipQuery
                {
                    filter = filter
                };
                var apiResponse = await _mediator.Send(command);
                if (apiResponse.Success == true)
                {
                    return Ok(apiResponse);
                }
                else return BadRequest(apiResponse);
            }
            catch (Exception ex)
            {

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tìm gói thành viên: {ex.Message}"));
            }
        }
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DetailMembershipDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetMembershipById([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                var command = new GetMembershipByIdQuery
                {
                    MembershipId = id,
                };
                var apiResponse = await _mediator.Send(command);
                if (apiResponse.Success == true)
                {
                    return Ok(apiResponse);
                }
                else return BadRequest(apiResponse);
            }
            catch (Exception ex)
            {

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tìm gói thành viên: {ex.Message}"));
            }
        }

    }
}
