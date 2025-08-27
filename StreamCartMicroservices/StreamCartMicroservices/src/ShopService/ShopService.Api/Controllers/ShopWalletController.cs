using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Models;
using ShopService.Application.Commands;
using ShopService.Application.Commands.WalletTransaction;
using ShopService.Application.DTOs.Membership;
using ShopService.Application.DTOs.WalletTransaction;
using ShopService.Application.Queries;
using ShopService.Application.Queries.WalletTransaction;
using ShopService.Domain.Entities;
using ShopService.Domain.Enums;

namespace ShopService.Api.Controllers
{
    [ApiController]
    [Route("api/shop-wallet")]
    public class ShopWalletController : Controller
    {
        private readonly IMediator _mediator;
        public ShopWalletController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]
        [Authorize(Roles = "OperationManager,Seller")]
        [ProducesResponseType(typeof(ApiResponse<WalletTransaction>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateWalletTransaction([FromBody] CreateWalletTransactionDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                string userId = User.FindFirst("id")?.Value;
                string? shopId = User.FindFirst("ShopId")?.Value;
                CreateWalletTraansactionCommand command = new CreateWalletTraansactionCommand()
                {
                    CreateWalletTransactionDTO = request,
                    ShopId = shopId,
                    UserId = userId
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

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tạo giao dịch ví: {ex.Message}"));
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "OperationManager,Seller")]
        [ProducesResponseType(typeof(ApiResponse<WalletTransaction>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateWalletTransaction([FromForm] WalletTransactionStatus status, [FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                string userId = User.FindFirst("id")?.Value;
                string? shopId = User.FindFirst("ShopId")?.Value;

                var command = new UpdateWalletTransactionCommand()
                {
                   WalletTransactionId = id,
                   Status = status,
                   ShopId=shopId,
                   UserId = userId
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

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi cập nhật giao dịch ví: {ex.Message}"));
            }
        }
        [HttpGet]
        [Authorize(Roles = "OperationManager,Seller")]
        [ProducesResponseType(typeof(ApiResponse<ListWalletransationDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> FilterWalletTransaction([FromQuery] FilterWalletTransactionDTO filter)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                string? shopId = User.FindFirst("ShopId")?.Value;

                var command = new FilterWalletTransactionQuery
                {
                    ShopId = shopId,
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

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tìm giao dịch ví  {ex.Message}"));
            }
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "OperationManager,Seller")]
        [ProducesResponseType(typeof(ApiResponse<WalletTransaction>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetWalletTransactionById([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                var command = new DetailWalletTransactionDTO
                {
                    Id = id
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

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tìm giao dịch ví: {ex.Message}"));
            }
        }
    }
}
