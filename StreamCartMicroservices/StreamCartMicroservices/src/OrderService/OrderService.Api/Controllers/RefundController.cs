using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.RefundDTOs;
using OrderService.Application.Interfaces.IServices;
using Shared.Common.Models;
using System;
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

        public RefundController(IRefundService refundService, ILogger<RefundController> logger)
        {
            _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    }
}