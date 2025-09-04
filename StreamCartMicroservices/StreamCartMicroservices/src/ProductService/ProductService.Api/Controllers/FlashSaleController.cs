using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands.CategoryCommands;
using ProductService.Application.Commands.FlashSaleCommands;
using ProductService.Application.DTOs.Category;
using ProductService.Application.DTOs.FlashSale;
using ProductService.Application.Helpers;
using ProductService.Application.Interfaces;
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
        private readonly IFlashSaleService _flashSaleService;

        public FlashSaleController(IMediator mediator, ICurrentUserService currentUserService, IFlashSaleService flashSaleService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _flashSaleService = flashSaleService;
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<List<DetailFlashSaleDTO>>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateFlashSale([FromBody] CreateFlashSaleDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));

                string userId = _currentUserService.GetUserId().ToString();
                string shopId = _currentUserService.GetShopId().ToString();

                if (string.IsNullOrEmpty(shopId))
                    return BadRequest(ApiResponse<object>.ErrorResult("Không tìm thấy thông tin shop"));

                var result = await _flashSaleService.CreateFlashSale(request, userId, shopId);

                if (result.Success)
                    return CreatedAtAction(nameof(GetFlashSaleById), new { id = result.Data.FirstOrDefault()?.Id }, result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tạo FlashSale: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DetailFlashSaleDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetFlashSaleById(string id)
        {
            try
            {
                var result = await _flashSaleService.GetFlashSaleById(id);

                if (result.Success)
                    return Ok(result);
                else
                    return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy FlashSale: {ex.Message}"));
            }
        }

        [HttpGet("shop")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<List<DetailFlashSaleDTO>>), 200)]
        public async Task<IActionResult> GetFlashSalesByShopAndDate(
            [FromQuery] DateTime? date = null,
            [FromQuery] int? slot = null)
        {
            try
            {
                string shopId = _currentUserService.GetShopId().ToString();
                if (string.IsNullOrEmpty(shopId))
                    return BadRequest(ApiResponse<object>.ErrorResult("Không tìm thấy thông tin shop"));

                var result = await _flashSaleService.GetFlashSalesByShopAndDateAsync(shopId, date, slot);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy FlashSale: {ex.Message}"));
            }
        }

        [HttpPut("{id}/products")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> UpdateFlashSaleProducts(
            string id,
            [FromBody] UpdateFlashSaleProductsDTO request)
        {
            try
            {
                string userId = _currentUserService.GetUserId().ToString();
                string shopId = _currentUserService.GetShopId().ToString();

                if (string.IsNullOrEmpty(shopId))
                    return BadRequest(ApiResponse<object>.ErrorResult("Không tìm thấy thông tin shop"));

                var result = await _flashSaleService.UpdateFlashSaleProductsAsync(
                    id, request.ProductIds, request.VariantIds, userId, shopId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi cập nhật sản phẩm FlashSale: {ex.Message}"));
            }
        }

        [HttpGet("products/available")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductWithoutFlashSaleDTO>>), 200)]
        public async Task<IActionResult> GetProductsWithoutFlashSale(
    [FromQuery] DateTime date,
    [FromQuery] int? slot = null)  
        {
            try
            {
                string shopId = _currentUserService.GetShopId().ToString();
                if (string.IsNullOrEmpty(shopId))
                    return BadRequest(ApiResponse<object>.ErrorResult("Không tìm thấy thông tin shop"));

                var result = await _flashSaleService.GetProductsWithoutFlashSaleAsync(shopId, date, slot);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy sản phẩm khả dụng: {ex.Message}"));
            }
        }

        [HttpGet("slots/available")]
        [ProducesResponseType(typeof(ApiResponse<List<int>>), 200)]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] DateTime date)
        {
            try
            {
                var result = await _flashSaleService.GetAvailableSlotsAsync(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy slot khả dụng: {ex.Message}"));
            }
        }

        /// <summary>
        /// API này đã được fix để xử lý pagination đúng cách
        /// </summary>
        [HttpGet("my-shop")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<List<DetailFlashSaleDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetMyShopFlashSales([FromQuery] FilterFlashSaleDTO filter)
        {
            try
            {
                string shopId = _currentUserService.GetShopId().ToString();
                if (string.IsNullOrEmpty(shopId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Không tìm thấy thông tin shop trong token"));
                }

                // Gọi trực tiếp service thay vì mediator để fix pagination
                var result = await _flashSaleService.GetFlashSalesByShopIdAsync(shopId, filter);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy danh sách FlashSale của shop: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<DetailFlashSaleDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateFlashSale([FromBody] UpdateFlashSaleDTO request, [FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhận vào không hợp lệ"));

            try
            {
                string? userId = _currentUserService.GetUserId().ToString();
                string shopId = _currentUserService.GetShopId().ToString();

                var result = await _flashSaleService.UpdateFlashSale(request, id, userId, shopId);

                if (result.Success == false)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi cập nhật FlashSale: {ex.Message}"));
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<DetailFlashSaleDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> FilterFlashSale([FromQuery] FilterFlashSaleDTO filter)
        {
            try
            {
                var result = await _flashSaleService.FilterFlashSale(filter);

                if (result.Success == true)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy danh sách FlashSale: {ex.Message}"));
            }
        }

        [HttpGet("detail/{id}")]
        [ProducesResponseType(typeof(ApiResponse<DetailFlashSaleDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetDetailFlashSale([FromRoute] string id)
        {
            try
            {
                var result = await _flashSaleService.GetFlashSaleById(id);

                if (result.Success == true)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tìm kiếm FlashSale: {ex.Message}"));
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
                string? userId = _currentUserService.GetUserId().ToString();
                string shopId = _currentUserService.GetShopId().ToString();

                var result = await _flashSaleService.DeleteFlashsale(id, userId, shopId);

                if (result.Success == true)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi xóa FlashSale: {ex.Message}"));
            }
        }

        [HttpGet("current")]
        [ProducesResponseType(typeof(ApiResponse<List<DetailFlashSaleDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetCurrentFlashSales()
        {
            try
            {
                var query = new GetCurrentFlashSalesQuery();
                var currentFlashSales = await _mediator.Send(query);

                if (currentFlashSales.Success)
                {
                    return Ok(currentFlashSales);
                }
                else
                {
                    return BadRequest(currentFlashSales);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy danh sách FlashSale hiện tại: {ex.Message}"));
            }
        }

        [HttpGet("debug")]
        [AllowAnonymous] // Tạm thời cho phép anonymous để test
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> DebugFlashSales()
        {
            try
            {
                // Lấy tất cả FlashSale để debug
                var allQuery = new GetAllFlashSaleQuery()
                {
                    IsActive = null, // Lấy tất cả
                    PageSize = 100,
                    PageIndex = 0
                };

                var allFlashSales = await _mediator.Send(allQuery);
                var now = DateTime.UtcNow;

                var debugInfo = new
                {
                    CurrentTime = now,
                    TotalFlashSales = allFlashSales.Data?.Count ?? 0,
                    FlashSales = allFlashSales.Data?.Select(fs => new
                    {
                        fs.Id,
                        fs.ProductId,
                        fs.StartTime,
                        fs.EndTime,
                        fs.IsActive,
                        IsCurrentlyActive = fs.StartTime <= now && fs.EndTime >= now && fs.IsActive,
                        TimeStatus = fs.StartTime > now ? "Chưa bắt đầu" :
                                   fs.EndTime < now ? "Đã kết thúc" : "Đang diễn ra"
                    }).ToList()
                };

                return Ok(ApiResponse<object>.SuccessResult(debugInfo, "Debug FlashSale data"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi debug: {ex.Message}"));
            }
        }

        /// <summary>
        /// API mới để lấy thông tin slots với thời gian cụ thể
        /// </summary>
        [HttpGet("slots/info")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> GetSlotsInfo([FromQuery] DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.Now.Date;
                var availableSlots = await _flashSaleService.GetAvailableSlotsAsync(targetDate);

                var slotsInfo = new
                {
                    Date = targetDate.ToString("dd/MM/yyyy"),
                    AvailableSlots = availableSlots.Data,
                    SlotDetails = FlashSaleSlotHelper.SlotTimeRanges.Select(s => new
                    {
                        Slot = s.Key,
                        TimeRange = $"{s.Value.Start:hh\\:mm} - {s.Value.End:hh\\:mm}",
                        IsAvailable = availableSlots.Data?.Contains(s.Key) ?? false
                    }).ToList()
                };

                return Ok(ApiResponse<object>.SuccessResult(slotsInfo, "Lấy thông tin slots thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy thông tin slots: {ex.Message}"));
            }
        }
        [HttpGet("shop/overview-simple")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<List<FlashSaleSlotSimpleDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetShopFlashSaleSimple()
        {
            try
            {
                string shopId = _currentUserService.GetShopId().ToString();
                if (string.IsNullOrEmpty(shopId))
                    return BadRequest(ApiResponse<object>.ErrorResult("Không tìm thấy thông tin shop"));

                var result = await _flashSaleService.GetShopFlashSaleSimpleAsync(shopId);

                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy thông tin tổng quan FlashSale: {ex.Message}"));
            }
        }
        [HttpDelete("slot")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> DeleteFlashSaleSlot([FromBody] DeleteFlashSaleSlotDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhận vào không hợp lệ"));

            try
            {
                string? userId = _currentUserService.GetUserId().ToString();
                string shopId = _currentUserService.GetShopId().ToString();

                var result = await _flashSaleService.DeleteFlashSaleSlotAsync(request, userId, shopId);

                if (result.Success == false)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi xóa FlashSale slot: {ex.Message}"));
            }
        }
        [HttpPatch("{id}/price-quantity")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<DetailFlashSaleDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateFlashSalePriceQuantity([FromBody] UpdateFlashSalePriceQuantityDTO request, [FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhận vào không hợp lệ"));

            try
            {
                string? userId = _currentUserService.GetUserId().ToString();
                string shopId = _currentUserService.GetShopId().ToString();

                var result = await _flashSaleService.UpdateFlashSalePriceQuantityAsync(request, id, userId, shopId);

                if (result.Success == false)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi cập nhật FlashSale: {ex.Message}"));
            }
        }
        [HttpPatch("{id}/sold")]
        [AllowAnonymous] // nếu chỉ cho service nội bộ gọi, có thể thêm auth/policy phù hợp
        [ProducesResponseType(typeof(ApiResponse<DetailFlashSaleDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateFlashSaleSold([FromRoute] string id, [FromBody] int quantity)
        {
            if (!ModelState.IsValid )
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));

            if (quantity <= 0)
                return BadRequest(ApiResponse<object>.ErrorResult("Số lượng mua phải lớn hơn 0"));

            try
            {
                var result = await _flashSaleService.UpdateFlashSaleStock(id, quantity);

                if (!result.Success)
                {
                    // Trả về 404 nếu không tìm thấy
                    if ((result.Message ?? string.Empty).Contains("Không tìm thấy FlashSale", StringComparison.OrdinalIgnoreCase))
                        return NotFound(ApiResponse<object>.ErrorResult(result.Message!));

                    return BadRequest(ApiResponse<object>.ErrorResult(result.Message!));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi cập nhật QuantitySold: {ex.Message}"));
            }
        }
    }
}