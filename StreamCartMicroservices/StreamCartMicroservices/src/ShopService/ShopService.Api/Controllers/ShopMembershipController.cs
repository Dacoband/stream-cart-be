using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Models;
using ShopService.Application.Commands;
using ShopService.Application.DTOs.Membership;
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

    }
}
