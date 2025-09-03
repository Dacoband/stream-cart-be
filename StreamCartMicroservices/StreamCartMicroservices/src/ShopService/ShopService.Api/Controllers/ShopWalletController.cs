using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Models;
using Shared.Common.Services.User;
using ShopService.Application.Commands;
using ShopService.Application.Commands.WalletTransaction;
using ShopService.Application.DTOs.Membership;
using ShopService.Application.DTOs.Wallet;
using ShopService.Application.DTOs.WalletTransaction;
using ShopService.Application.Interfaces;
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
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ShopWalletController> _logger;
        private readonly IWalletService _walletService;
        public ShopWalletController(IMediator mediator, ICurrentUserService currentUserService, ILogger<ShopWalletController> logger, IWalletService walletService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
            _walletService = walletService;
        }
        [HttpPost]
        //[Authorize(Roles = "OperationManager,Seller")]
        [ProducesResponseType(typeof(ApiResponse<WalletTransaction>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateWalletTransaction([FromBody] CreateWalletTransactionDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                string userId = _currentUserService.GetUserId().ToString();
                string? shopId = Request.Headers["X-Shop-Id"].FirstOrDefault()
                                ?? User.FindFirst("ShopId")?.Value
                                ?? _currentUserService.GetShopId();

                if (string.IsNullOrEmpty(shopId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Không tìm thấy thông tin Shop ID"));
                }

                if (string.IsNullOrEmpty(userId))
                {
                    // ✅ Get from header if current user service fails
                    userId = Request.Headers["X-User-Id"].FirstOrDefault() ?? "system";
                }
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
        [HttpPatch("{id}")]
       // [Authorize(Roles = "OperationManager,Seller")]
        [ProducesResponseType(typeof(ApiResponse<WalletTransaction>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateWalletTransaction([FromForm] WalletTransactionStatus status, [FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                string userId = _currentUserService.GetUserId().ToString();
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
        //[Authorize(Roles = "OperationManager,Seller")]
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
       // [Authorize(Roles = "OperationManager,Seller")]
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
        /// <summary>
        /// Cập nhật balance của wallet
        /// </summary>
        [HttpPatch("shop/{shopId}/balance")]
        [Authorize]
        public async Task<IActionResult> UpdateWalletBalance(Guid shopId, [FromBody] UpdateWalletBalanceRequest request)
        {
            try
            {
                var wallet = await _walletService.GetWalletByShopIdAsync(shopId);
                if (wallet == null)
                {
                    return NotFound(new { error = "Không tìm thấy ví của shop" });
                }

                var result = await UpdateWalletBalanceDirectly(wallet.Id, request.Amount, request.ModifiedBy ?? "System");

                if (result)
                {
                    return Ok(new { success = true, message = "Cập nhật balance thành công" });
                }

                return BadRequest(new { error = "Cập nhật balance thất bại" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật balance cho shop {ShopId}", shopId);
                return StatusCode(500, new { error = "Lỗi hệ thống" });
            }
        }
        private async Task<bool> UpdateWalletBalanceDirectly(Guid walletId, decimal amount, string modifiedBy)
        {
            try
            {
                var result = await _walletService.AddFundsAsync(walletId, amount, modifiedBy);

                if (result)
                {
                    _logger.LogInformation("✅ Cập nhật balance thành công cho wallet {WalletId}, amount: {Amount}", walletId, amount);
                }
                else
                {
                    _logger.LogWarning("❌ Cập nhật balance thất bại cho wallet {WalletId}", walletId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật balance cho wallet {WalletId}", walletId);
                return false;
            }
        }
        /// <summary>
        /// Lấy danh sách giao dịch ví của user hiện tại
        /// </summary>
        [HttpGet("user/transactions")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ListWalletransationDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetUserWalletTransactions([FromQuery] FilterWalletTransactionDTO filter)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                Guid userId = _currentUserService.GetUserId();

                if (userId == Guid.Empty)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Không tìm thấy thông tin User ID"));
                }

                var query = new GetUserWalletTransactionQuery
                {
                    UserId = userId,
                    Filter = filter ?? new FilterWalletTransactionDTO()
                };

                var apiResponse = await _mediator.Send(query);

                if (apiResponse.Success)
                {
                    return Ok(apiResponse);
                }
                else
                {
                    return BadRequest(apiResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy giao dịch ví của user {UserId}", _currentUserService.GetUserId());
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy giao dịch ví của user: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy giao dịch ví của user cụ thể (cho admin)
        /// </summary>
        [HttpGet("admin/user/{userId}/transactions")]
        [Authorize(Roles = "Admin,OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<ListWalletransationDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetUserWalletTransactionsByAdmin(Guid userId, [FromQuery] FilterWalletTransactionDTO filter)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                var query = new GetUserWalletTransactionQuery
                {
                    UserId = userId,
                    Filter = filter ?? new FilterWalletTransactionDTO()
                };

                var apiResponse = await _mediator.Send(query);

                if (apiResponse.Success)
                {
                    return Ok(apiResponse);
                }
                else
                {
                    return BadRequest(apiResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi admin lấy giao dịch ví của user {UserId}", userId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy giao dịch ví của user: {ex.Message}"));
            }
        }
        public class UpdateWalletBalanceRequest
        {
            public decimal Amount { get; set; }
            public string? ModifiedBy { get; set; }
        }
    }
}
