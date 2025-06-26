using DeliveryService.Application.DTOs.AddressDTOs;
using DeliveryService.Application.DTOs.DeliveryOrder;
using DeliveryService.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Models;

namespace DeliveryService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GHNAdressController : ControllerBase
    {
        private readonly IDeliveryAddressInterface _addressService;
        public GHNAdressController(IDeliveryAddressInterface addressService)
        {
            _addressService = addressService;
        }
        [HttpPost("create-ghn-order")]
        public async Task<IActionResult> CreateOrder(
     [FromBody] UserCreateOrderRequest input)
        {
            var result = await _addressService.CreateOrderAsync(input);
            if(result.Success) { return Ok(result); }
            else return BadRequest(result);
        }
        [HttpPost("preview-order")]
        public async Task<IActionResult> PreviewOrder([FromBody] UserPreviewOrderRequestDTO input)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                    Data = null
                });

            var result = await _addressService.PreviewOrder(input);
            return Ok(result);
        }
        [HttpGet("order-log/{orderCode}")]
        public async Task<IActionResult> GetOrderLog(string orderCode)
        {
            var result = await _addressService.GetDeliveryStatus(orderCode);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
