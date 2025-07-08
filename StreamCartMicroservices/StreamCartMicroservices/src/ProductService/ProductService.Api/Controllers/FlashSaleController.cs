using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands.CategoryCommands;
using ProductService.Application.Commands.FlashSaleCommands;
using ProductService.Application.DTOs.Category;
using ProductService.Application.DTOs.FlashSale;
using ProductService.Application.Queries.FlashSaleQueries;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System.Security.Claims;

namespace ProductService.Api.Controllers
{
    [Route("api/flashsales")]
    [ApiController]
    public class FlashSaleController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        public FlashSaleController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }
        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<List<FlashSale>>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateFlashSale([FromBody] CreateFlashSaleDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhận vào không hợp lệ"));

            try
            {
                string? userId = _currentUserService.GetUserId().ToString() ?? "123";
                string? shopId = User.FindFirst("ShopId")?.Value ;
                var command = new CreateFlashSaleCommand()
                {
                    UserId = userId ?? "123",
                    ShopId = shopId ?? "123",
                    ProductId = request.ProductId,
                    VariantId = request.VariantId,
                    QuantityAvailable = request.QuantityAvailable,
                    FLashSalePrice = request.FLashSalePrice,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                };

                var createdFlashSale = await _mediator.Send(command);
                if (createdFlashSale.Success == false)
                {
                    return BadRequest(createdFlashSale);
                }
                return Created(request.ProductId.ToString(), createdFlashSale);
            }
            catch (Exception ex)
            {

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tạo FlashSale"));
            }


        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<FlashSale>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateFlashSale([FromBody] UpdateFlashSaleDTO request, [FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhận vào không hợp lệ"));

            try
            {
                string? userId = _currentUserService.GetUserId().ToString() ?? "123";
                string? shopId = User.FindFirst("ShopId")?.Value;
                var command = new UpdateFlashSaleCommand()
                {
                    UserId = userId,
                    ShopId = shopId,
                    FlashSaleId = id,
                    QuantityAvailable = request.QuantityAvailable,
                    FLashSalePrice = request.FLashSalePrice,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                };

                var updatedFlashSale = await _mediator.Send(command);
                if (updatedFlashSale.Success == false)
                {
                    return BadRequest(updatedFlashSale);
                }
                return Ok(updatedFlashSale);
            }
            catch (Exception ex)
            {

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tạo FlashSale"));
            }


        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<DetailFlashSaleDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> FilterFlashSale([FromQuery] FilterFlashSaleDTO filterFlashSaleDTO)
        {
            try
            {
                var query = new GetAllFlashSaleQuery()
                {
                    ProductId = filterFlashSaleDTO.ProductId,
                    VariantId = filterFlashSaleDTO.VariantId,
                    StartDate = filterFlashSaleDTO.StartDate,
                    EndDate = filterFlashSaleDTO.EndDate,
                    IsActive = filterFlashSaleDTO.IsActive,
                    OrderBy = filterFlashSaleDTO.OrderBy,
                    OrderDirection = filterFlashSaleDTO.OrderDirection,
                    PageSize = filterFlashSaleDTO.PageSize,
                    PageIndex = filterFlashSaleDTO.PageIndex,
                };
                var filterFlashSale = await _mediator.Send(query);
                if (filterFlashSale.Success == true)
                {
                    return Ok(filterFlashSale);
                }
                else return BadRequest(filterFlashSale);

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy danh sách FlashSale"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DetailFlashSaleDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetDetailFlashSale([FromRoute] string id)
        {
            try
            {
                var query = new GetDetailFlashSaleQuery()
                {
                    FlashSaleId = id
                };
                var detailFlashSale = await _mediator.Send(query);
                if (detailFlashSale.Success == true)
                {
                    return Ok(detailFlashSale);
                }
                else return BadRequest(detailFlashSale);

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tìm kiếm FlashSale"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> DeleteFlashSale([FromRoute] string id)
        {
            try
            {
                string? userId = _currentUserService.GetUserId().ToString() ?? "123";
                string? shopId = User.FindFirst("ShopId")?.Value;
                var command = new DeleteFlashSaleCommand()
                {
                    FlashSaleId = id,
                    ShopId = shopId ?? string.Empty,
                    UserId = userId
                };
                var deletedFlashSale = await _mediator.Send(command);
                if (deletedFlashSale.Success == true)
                {
                    return Ok(deletedFlashSale);
                }
                else return BadRequest(deletedFlashSale);

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi xóa FlashSale"));
            }
        }
    }
}
