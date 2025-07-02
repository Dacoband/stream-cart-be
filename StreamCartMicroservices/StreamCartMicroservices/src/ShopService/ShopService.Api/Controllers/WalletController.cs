using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShopService.Application.DTOs.Wallet;
using ShopService.Application.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShopService.Api.Controllers
{
    [ApiController]
    [Route("api/wallets")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(IWalletService walletService, ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WalletDTO>> GetWallet(Guid id)
        {
            var wallet = await _walletService.GetWalletByIdAsync(id);
            if (wallet == null)
                return NotFound();

            return Ok(wallet);
        }

        [HttpGet("shop/{shopId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WalletDTO>> GetWalletByShopId(Guid shopId)
        {
            var wallet = await _walletService.GetWalletByShopIdAsync(shopId);
            if (wallet == null)
                return NotFound();

            return Ok(wallet);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,System")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WalletDTO>> CreateWallet([FromBody] CreateWalletDTO createWalletDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var wallet = await _walletService.CreateWalletAsync(createWalletDTO, userId);
                return CreatedAtAction(nameof(GetWallet), new { id = wallet.Id }, wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo ví mới");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("shop/{shopId}/banking-info")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WalletDTO>> UpdateShopWalletBankingInfo(Guid shopId, [FromBody] UpdateWalletDTO updateWalletDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Tìm ví theo shopId
                var wallet = await _walletService.GetWalletByShopIdAsync(shopId);
                if (wallet == null)
                    return NotFound(new { error = "Không tìm thấy ví cho shop này" });

                // Cập nhật thông tin ngân hàng
                var updated = await _walletService.UpdateWalletAsync(wallet.Id, updateWalletDTO, userId);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin ngân hàng cho ví của shop {ShopId}", shopId);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("shop-payment")]
        [Authorize(Roles = "System")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessShopPayment([FromBody] ShopPaymentDTO paymentRequest)
        {
            try
            {
                var result = await _walletService.ProcessShopPaymentAsync(paymentRequest);
                if (!result)
                    return BadRequest(new { error = "Xử lý thanh toán thất bại" });

                return Ok(new { success = true, message = "Thanh toán thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý thanh toán cho shop {ShopId}, đơn hàng {OrderId}",
                    paymentRequest.ShopId, paymentRequest.OrderId);
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}