using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.RefundDTOs;
using OrderService.Application.Interfaces.IServices;
using OrderService.Domain.Enums;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OrderService.Api.Controllers
{
    [Route("api/refunds")]
    [ApiController]
    //[Authorize]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _refundService;
        private readonly ILogger<RefundController> _logger;
        private readonly ICurrentUserService _currentUserService;

        public RefundController(IRefundService refundService, ILogger<RefundController> logger, ICurrentUserService currentUserService)
        {
            _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// Creates a new refund request
        /// </summary>
        /// <param name="createRefundDto">Refund request creation data</param>
        /// <returns>Created refund request</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RefundRequestDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateRefundRequest([FromBody] CreateRefundRequestDto createRefundDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Invalid input data", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
                }

                var refundRequest = await _refundService.CreateRefundRequestAsync(createRefundDto);

                return Created($"/api/refunds/{refundRequest.Id}",
                    ApiResponse<RefundRequestDto>.SuccessResult(refundRequest, "Refund request created successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund request for order {OrderId}", createRefundDto.OrderId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ErrorResult("An error occurred while creating the refund request"));
            }
        }

        /// <summary>
        /// Updates refund request status
        /// </summary>
        /// <param name="updateStatusDto">Status update data</param>
        /// <returns>Updated refund request</returns>
        [HttpPut("status")]
        [ProducesResponseType(typeof(ApiResponse<RefundRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRefundStatus([FromBody] UpdateRefundStatusDto updateStatusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Invalid input data", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
                }

                var refundRequest = await _refundService.UpdateRefundStatusAsync(updateStatusDto);

                return Ok(ApiResponse<RefundRequestDto>.SuccessResult(refundRequest, "Refund status updated successfully"));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund status for refund {RefundRequestId}", updateStatusDto.RefundRequestId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ErrorResult("An error occurred while updating the refund status"));
            }
        }

        /// <summary>
        /// Gets refund request by ID
        /// </summary>
        /// <param name="id">Refund request ID</param>
        /// <returns>Refund request details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RefundRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRefundRequestById(Guid id)
        {
            try
            {
                var refundRequest = await _refundService.GetRefundRequestByIdAsync(id);
                if (refundRequest == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Refund request not found"));
                }

                return Ok(ApiResponse<RefundRequestDto>.SuccessResult(refundRequest, "Refund request retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refund request with ID {RefundRequestId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ErrorResult("An error occurred while retrieving the refund request"));
            }
        }

        [HttpGet("shop/{shopId}")]
        [Authorize(Roles = "Seller,OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RefundRequestDto>>), 200)]
        public async Task<IActionResult> GetShopRefunds(
            Guid shopId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] RefundStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var result = await _refundService.GetRefundRequestsByShopIdAsync(
                    shopId, pageNumber, pageSize, status, fromDate, toDate);

                return Ok(ApiResponse<PagedResult<RefundRequestDto>>.SuccessResult(
                    result, "Lấy danh sách hoàn tiền thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shop refunds for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }
        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "Seller,OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<RefundRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> ConfirmRefund(
            Guid id,
            [FromBody] ConfirmRefundDto request)
        {
            try
            {
                var modifiedBy = _currentUserService.GetUserId().ToString();

                var result = await _refundService.ConfirmRefundRequestAsync(
                    id, request.IsApproved, request.Reason, modifiedBy);

                var message = request.IsApproved ? "Xác nhận hoàn tiền thành công" : "Từ chối hoàn tiền thành công";
                return Ok(ApiResponse<RefundRequestDto>.SuccessResult(result, message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming refund {RefundId}", id);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }
    }
    public class ConfirmRefundDto
    {
        [Required]
        public bool IsApproved { get; set; }

        public string? Reason { get; set; }
    }
}