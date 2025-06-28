using CartService.Application.Command;
using CartService.Application.DTOs;
using CartService.Application.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Models;
using System.Security.Claims;

namespace CartService.Api.Controllers
{

    [Route("api/carts")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CartController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]
        //[Authorize]
        [ProducesResponseType(typeof(ApiResponse<CreateCartDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> AddToCart([FromBody] CreateCartDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var command = new AddToCartCommand()
                {
                    ProductId = request.ProductId,
                    VariantId = request.VariantId,
                    Quantity = request.Quantity,
                    UserId = userId ?? "123"
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

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi thêm sản phẩm vào giỏ hàng: {ex.Message}"));
            }
        }
        [HttpGet]
        //[Authorize(Roles ="Customer")]
        [ProducesResponseType(typeof(ApiResponse<CartResponeDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetMyCart()
        {

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "123";
                var command = new GetMyCartQuery()
                {
                    userId = userId ?? "123",
                };
                var apiResponse = await _mediator.Send(command);
                if (apiResponse.Success == true) { return Ok(apiResponse); }
                else return BadRequest(apiResponse);
            }
            catch (Exception ex) {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tìm giỏ hàng: {ex.Message}"));
            }
        }
        [HttpGet("/PreviewOrder")]
        //[Authorize(Roles ="Customer")]
        [ProducesResponseType(typeof(ApiResponse<PreviewOrderResponseDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> PreviewOrder([FromQuery]PreviewOrderRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));
            try
            {
                var command = new PreviewOrderQuery()
                {
                    CartItemId = request.CartItemId,
                };
                var apiResponse = await _mediator.Send(command);
                if (apiResponse.Success == true) { return Ok(apiResponse); }
                else return BadRequest(apiResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tìm giỏ hàng"));
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartItem([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));
            try
            {
                var command = new DeleteCartItemCommand()
                {
                    CartItemId = id,
                };
                var apiResponse = await _mediator.Send(command);
                if (apiResponse.Success == true) { return Ok(apiResponse); }
                else return BadRequest(apiResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi xóa sản phẩm trong giỏ hàng"));
            }
        }
        [HttpPut]
        //[Authorize(Roles = "Customer")]
        [ProducesResponseType(typeof(ApiResponse<PreviewOrderResponseDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "123";
            try { 
            var command = new UpdateCartItemCommand()
            {
                UserId = userId,
                CartItemId = request.CartItem,
                Quantity = request.Quantity,
                VariantId = request.VariantId
            };
            var apiResponse = await _mediator.Send(command);
            if (apiResponse.Success == true) { return Ok(apiResponse); }
                else return BadRequest(apiResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi cập nhật sản phẩm trong giỏ hàng"));
            }
        }
    }
    

}


