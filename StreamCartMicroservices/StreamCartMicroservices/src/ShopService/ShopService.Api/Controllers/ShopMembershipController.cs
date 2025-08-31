using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Models;
using Shared.Common.Services.User;
using ShopService.Application.Commands;
using ShopService.Application.Commands.ShopMembership;
using ShopService.Application.DTOs.Membership;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries;
using ShopService.Domain.Entities;

namespace ShopService.Api.Controllers
{
    [ApiController]
    [Route("api/shopmembership")]
    public class ShopMembershipController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IShopMembershipService _shopMembershipService;
        private readonly ICurrentUserService _currentUserService;

        public ShopMembershipController(IMediator mediator, IShopMembershipService shopMembershipService, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _shopMembershipService = shopMembershipService;
            _currentUserService = currentUserService;
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
                string userId = _currentUserService.GetUserId().ToString();
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
                string userId = _currentUserService.GetUserId().ToString();
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
                string userId = _currentUserService.GetUserId().ToString();
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
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DetailShopMembershipDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetShopMembershipById([FromRoute] string id)
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
        [HttpGet("shop/{shopId}/active")]
        [AllowAnonymous] 
        [ProducesResponseType(typeof(ApiResponse<ShopMembershipDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetActiveShopMembership(Guid shopId)
        {
            try
            {
                var filter = new FilterShopMembership
                {
                    ShopId = shopId.ToString(),
                    Status = "Ongoing",
                    PageIndex = 1,
                    PageSize = 1
                };

                var result = await _shopMembershipService.FilterShopMembership(filter);

                if (!result.Success || result.Data?.DetailShopMembership == null || !result.Data.DetailShopMembership.Any())
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy gói thành viên đang hoạt động"));
                }

                var membership = result.Data.DetailShopMembership.First();

                // Map to ShopMembershipDto for LivestreamService
                var membershipDto = new ShopMembershipDto
                {
                    Id = Guid.Parse(membership.Id),
                    ShopId = membership.ShopID,
                    StartDate = membership.StartDate,
                    EndDate = membership.EndDate,
                    RemainingLivestream = (int)membership.RemainingLivestream,
                    Status = membership.Status,
                    MaxProduct = membership.MaxProduct ?? 0,
                    Commission = membership.Commission ?? 0,
                    CreatedAt = membership.CreatedAt, 
                    LastModifiedAt = membership.ModifiedAt,
                    IsDeleted = membership.IsDeleted
                };

                return Ok(ApiResponse<ShopMembershipDto>.SuccessResult(membershipDto, "Lấy gói thành viên thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy gói thành viên: {ex.Message}"));
            }
        }
        [HttpPut("shop/{shopId}/remaining-livestream")]
        [AllowAnonymous] 
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateRemainingLivestream(Guid shopId, [FromBody] UpdateRemainingLivestreamRequest request)
        {
            try
            {
                if (!ModelState.IsValid || request == null)
                    return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
                var filter = new FilterShopMembership
                {
                    ShopId = shopId.ToString(),
                    Status = "Ongoing",
                    PageIndex = 1,
                    PageSize = 1
                };

                var membershipResult = await _shopMembershipService.FilterShopMembership(filter);

                if (!membershipResult.Success || membershipResult.Data?.DetailShopMembership == null || !membershipResult.Data.DetailShopMembership.Any())
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy gói thành viên đang hoạt động"));
                }

                var membership = membershipResult.Data.DetailShopMembership.First();

                var updateResult = await _shopMembershipService.UpdateShopMembership(membership.Id, request.RemainingLivestream);

                if (!updateResult.Success)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult(updateResult.Message));
                }

                return Ok(ApiResponse<bool>.SuccessResult(true, "Cập nhật thời gian livestream thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi cập nhật thời gian livestream: {ex.Message}"));
            }
        }
    }
}
