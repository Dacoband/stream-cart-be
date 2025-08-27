using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using Shared.Common.Services.User;
using ShopService.Application.Commands.Voucher;
using ShopService.Application.DTOs.Voucher;
using ShopService.Application.Queries.Voucher;
using ShopService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopService.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class ShopVoucherController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ShopVoucherController> _logger;

        public ShopVoucherController(
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<ShopVoucherController> logger)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
        }
        [HttpPost("vouchers/available")]
        [AllowAnonymous] // Customer không cần đăng nhập để xem voucher
        [ProducesResponseType(typeof(ApiResponse<List<CustomerVoucherResponseDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<List<CustomerVoucherResponseDto>>> GetAvailableVouchersForCustomer([FromBody] CustomerVoucherRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));

                _logger.LogInformation("Customer requesting available vouchers for amount: {OrderAmount}đ, ShopId: {ShopId}",
                    request.OrderAmount, request.ShopId);

                var query = new GetAvailableVouchersForCustomerQuery
                {
                    OrderAmount = request.OrderAmount,
                    ShopId = request.ShopId,
                    SortByDiscountDesc = request.SortByDiscountDesc
                };

                var result = await _mediator.Send(query);

                var message = result.Any()
                    ? $"Tìm thấy {result.Count} voucher khả dụng cho đơn hàng {request.OrderAmount:N0}đ"
                    : $"Không có voucher nào khả dụng cho đơn hàng {request.OrderAmount:N0}đ";

                return Ok(ApiResponse<List<CustomerVoucherResponseDto>>.SuccessResult(result, message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting available vouchers for customer");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        [HttpGet("vouchers/available")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<CustomerVoucherResponseDto>>), 200)]
        public async Task<ActionResult<List<CustomerVoucherResponseDto>>> GetAvailableVouchersForCustomerGet(
            [FromQuery] decimal orderAmount,
            [FromQuery] Guid? shopId = null,
            [FromQuery] int limit = 10,
            [FromQuery] VoucherType? voucherType = null,
            [FromQuery] bool sortByDiscountDesc = true)
        {
            try
            {
                if (orderAmount <= 0)
                    return BadRequest(ApiResponse<object>.ErrorResult("Số tiền đơn hàng phải lớn hơn 0"));

                var query = new GetAvailableVouchersForCustomerQuery
                {
                    OrderAmount = orderAmount,
                    ShopId = shopId,
                    SortByDiscountDesc = sortByDiscountDesc
                };

                var result = await _mediator.Send(query);

                var message = result.Any()
                    ? $"Tìm thấy {result.Count} voucher khả dụng"
                    : "Không có voucher nào khả dụng";

                return Ok(ApiResponse<List<CustomerVoucherResponseDto>>.SuccessResult(result, message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting available vouchers for customer");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách voucher active của shop
        /// </summary>
        [HttpGet("vouchers/shop/{shopId}")]
        [ProducesResponseType(typeof(ApiResponse<List<ShopVoucherDto>>), 200)]
        public async Task<ActionResult<List<ShopVoucherDto>>> GetActiveVouchersByShop(Guid shopId)
        {
            try
            {
                var query = new GetActiveVouchersByShopQuery { ShopId = shopId };
                var result = await _mediator.Send(query);
                return Ok(ApiResponse<List<ShopVoucherDto>>.SuccessResult(result, "Lấy danh sách voucher thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy voucher active cho shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Validate voucher theo code
        /// </summary>
        [HttpGet("vouchers/{code}/validate")]
        [ProducesResponseType(typeof(ApiResponse<VoucherValidationDto>), 200)]
        public async Task<ActionResult<VoucherValidationDto>> ValidateVoucher(string code, [FromQuery] decimal orderAmount, [FromQuery] Guid? shopId = null)
        {
            try
            {
                var query = new ValidateVoucherQuery
                {
                    Code = code,
                    OrderAmount = orderAmount,
                    ShopId = shopId
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<VoucherValidationDto>.SuccessResult(result, "Kiểm tra voucher thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate voucher {Code}", code);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Áp dụng voucher cho đơn hàng
        /// </summary>
        [HttpPost("vouchers/{code}/apply")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<VoucherApplicationDto>), 200)]
        public async Task<ActionResult<VoucherApplicationDto>> ApplyVoucher(string code, [FromBody] ApplyVoucherDto request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var command = new ApplyVoucherCommand
                {
                    Code = code,
                    OrderAmount = request.OrderAmount,
                    OrderId = request.OrderId,
                    UserId = userId,
                    ShopId = _currentUserService.GetShopId() != null ? Guid.Parse(_currentUserService.GetShopId()) : null
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<VoucherApplicationDto>.SuccessResult(result, "Áp dụng voucher thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi áp dụng voucher {Code}", code);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Tạo voucher mới
        /// </summary>
        [HttpPost("vouchers")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<ShopVoucherDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ShopVoucherDto>> CreateVoucher([FromBody] CreateShopVoucherDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));

            try
            {
                var userId = _currentUserService.GetUserId();
                // Lấy shop ID từ user (giả sử có method này)
                var shopId = Guid.Parse(_currentUserService.GetShopId());

                var command = new CreateShopVoucherCommand
                {
                    ShopId = shopId,
                    Code = request.Code,
                    Description = request.Description,
                    Type = request.Type,
                    Value = request.Value,
                    MaxValue = request.MaxValue,
                    MinOrderAmount = request.MinOrderAmount,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    AvailableQuantity = request.AvailableQuantity,
                    CreatedBy = userId.ToString()
                };

                var result = await _mediator.Send(command);
                return Created($"/api/vouchers/{result.Id}",
                    ApiResponse<ShopVoucherDto>.SuccessResult(result, "Tạo voucher thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo voucher");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cập nhật voucher
        /// </summary>
        [HttpPut("vouchers/{id}")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<ShopVoucherDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ShopVoucherDto>> UpdateVoucher(Guid id, [FromBody] UpdateShopVoucherDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));

            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new UpdateShopVoucherCommand
                {
                    Id = id,
                    Description = request.Description,
                    Value = request.Value,
                    MaxValue = request.MaxValue,
                    MinOrderAmount = request.MinOrderAmount,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    AvailableQuantity = request.AvailableQuantity,
                    UpdatedBy = userId.ToString()
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<ShopVoucherDto>.SuccessResult(result, "Cập nhật voucher thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật voucher {VoucherId}", id);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Xóa voucher
        /// </summary>
        [HttpDelete("vouchers/{id}")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<ActionResult> DeleteVoucher(Guid id)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var command = new DeleteShopVoucherCommand
                {
                    Id = id,
                    DeletedBy = userId.ToString()
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Xóa voucher thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa voucher {VoucherId}", id);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách voucher của shop người dùng hiện tại
        /// </summary>
        [HttpGet("vouchers/my-shop")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ShopVoucherDto>>), 200)]
        public async Task<ActionResult<PagedResult<ShopVoucherDto>>> GetMyShopVouchers([FromQuery] VoucherFilterDto filter)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var query = new GetMyShopVouchersQuery
                {
                    UserId = userId,
                    Filter = filter
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<PagedResult<ShopVoucherDto>>.SuccessResult(result, "Lấy danh sách voucher thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy voucher của shop người dùng");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Kích hoạt voucher
        /// </summary>
        [HttpPost("vouchers/{id}/activate")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<ActionResult> ActivateVoucher(Guid id)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var command = new ActivateVoucherCommand
                {
                    Id = id,
                    ModifiedBy = userId.ToString()
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Kích hoạt voucher thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kích hoạt voucher {VoucherId}", id);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Vô hiệu hóa voucher
        /// </summary>
        [HttpPost("vouchers/{id}/deactivate")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<ActionResult> DeactivateVoucher(Guid id)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var command = new DeactivateVoucherCommand
                {
                    Id = id,
                    ModifiedBy = userId.ToString()
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Vô hiệu hóa voucher thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi vô hiệu hóa voucher {VoucherId}", id);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thống kê sử dụng voucher
        /// </summary>
        [HttpGet("vouchers/{id}/usage-statistics")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<VoucherUsageStatsDto>), 200)]
        public async Task<ActionResult<VoucherUsageStatsDto>> GetVoucherUsageStatistics(Guid id)
        {
            try
            {
                var query = new GetVoucherUsageStatsQuery { VoucherId = id };
                var result = await _mediator.Send(query);
                return Ok(ApiResponse<VoucherUsageStatsDto>.SuccessResult(result, "Lấy thống kê voucher thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê voucher {VoucherId}", id);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách voucher của shop (cho admin route cũ)
        /// </summary>
        [HttpGet("shops/{shopId}/vouchers")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ShopVoucherDto>>), 200)]
        public async Task<IActionResult> GetShopVouchers(
            Guid shopId,
            [FromQuery] bool? isActive = null,
            [FromQuery] VoucherType? type = null,
            [FromQuery] bool? isExpired = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new GetShopVouchersQuery
                {
                    ShopId = shopId,
                    IsActive = isActive,
                    Type = type,
                    IsExpired = isExpired,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<PagedResult<ShopVoucherDto>>.SuccessResult(result, "Lấy danh sách voucher thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách voucher cho shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
    }
}