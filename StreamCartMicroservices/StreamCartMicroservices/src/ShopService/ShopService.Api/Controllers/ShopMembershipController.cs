using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Models;
using ShopService.Application.Commands;
using ShopService.Application.Commands.ShopMembership;
using ShopService.Application.DTOs.Membership;
using ShopService.Application.Queries;
using ShopService.Domain.Entities;

namespace ShopService.Api.Controllers
{
    [ApiController]
    [Route("api/shopmembership")]
    public class ShopMembershipController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ShopMembershipController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]
        [Authorize(Roles ="Seller")]
        [ProducesResponseType(typeof(ApiResponse<ShopMembership>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateShopMembership([FromBody] string membershipId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                string userId = User.FindFirst("id")?.Value;
                PurchaseShopMembershipCommand command = new PurchaseShopMembershipCommand()
                {
                    MembershipId = membershipId,
                    UserId = userId,
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

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi mua gói thành viên {ex.Message}"));
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<DetailShopMembershipDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> DeactiveShopMembership([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                string userId = User.FindFirst("id")?.Value;
               DeleteShopMembershipCommand command = new DeleteShopMembershipCommand()
                {
                    ShopMembershipId = id,
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

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi xóa gói thành viên của shop  {ex.Message}"));
            }
        }
        [HttpPatch]
        [ProducesResponseType(typeof(ApiResponse<DetailShopMembershipDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateShopMembership([FromBody] UpdateShopMembershipDTO request )
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                string userId = User.FindFirst("id")?.Value;
                UpdateShopMembershipCommand command = new UpdateShopMembershipCommand()
                {
                    RemainingLivestream = request.RemainingLivstream,
                    ShopId = request.ShopId,
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

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi cập nhật gói thành viên của cửa hàng  {ex.Message}"));
            }
        }
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<DetailShopMembershipDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetShopMembershipById([FromQuery] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                DetailShopMembershipQuery command = new DetailShopMembershipQuery()
                {
                    ShopMembershipId = id,
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

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tìm gói thành viên của cửa hàng  {ex.Message}"));
            }
        }
        [HttpGet("filter")]
        [ProducesResponseType(typeof(ApiResponse<ListShopMembershipDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> FilterShopMembership([FromQuery] FilterShopMembership filter)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                FilterShopMembershipQuery command = new FilterShopMembershipQuery()
                {
                    Filter = filter
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

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tìm gói thành viên của cửa hàng  {ex.Message}"));
            }
        }


    }
}
