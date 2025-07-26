using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Common.Domain.Bases;
using Shared.Common.Services.User;
using ShopService.Application.Commands;
using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;
using ShopService.Domain.Enums;

namespace ShopService.Api.Controllers
{
    [ApiController]
    [Route("api/shops")]
    public class ShopController : ControllerBase
    {
        private readonly IShopManagementService _shopManagementService;
        private readonly ILogger<ShopController> _logger;
        private readonly ICurrentUserService _currentUserService;

        public ShopController(IShopManagementService shopManagementService, ILogger<ShopController> logger, ICurrentUserService currentUserService)
        {
            _shopManagementService = shopManagementService;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        #region Public Endpoints

        /// <summary>
        /// Lấy thông tin shop theo ID
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <returns>Thông tin shop</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShopDto>> GetShopById(Guid id)
        {
            var shop = await _shopManagementService.GetShopByIdAsync(id);
            if (shop == null)
                return NotFound();

            return Ok(shop);
        }

        /// <summary>
        /// Tìm kiếm shop theo tên
        /// </summary>
        /// <param name="query">Từ khóa tìm kiếm</param>
        /// <returns>Danh sách shop phù hợp</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ShopDto>>> SearchShops([FromQuery] string query)
        {
            var shops = await _shopManagementService.SearchShopsByNameAsync(query);
            return Ok(shops);
        }

        /// <summary>
        /// Lấy danh sách shop phân trang
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ShopDto>>> GetShops(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? approvalStatus = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool ascending = true)
        {
            // Parse status enum if provided
            ShopStatus? shopStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ShopStatus>(status, true, out var parsedStatus))
                shopStatus = parsedStatus;

            // Parse approval status enum if provided
            ApprovalStatus? shopApprovalStatus = null;
            if (!string.IsNullOrEmpty(approvalStatus) && Enum.TryParse<ApprovalStatus>(approvalStatus, true, out var parsedApprovalStatus))
                shopApprovalStatus = parsedApprovalStatus;

            var pagedShops = await _shopManagementService.GetShopsPagedAsync(
                pageNumber,
                pageSize,
                shopStatus,
                shopApprovalStatus,
                searchTerm,
                sortBy,
                ascending);

            return Ok(pagedShops);
        }

        #endregion

        #region Customer Endpoints

        /// <summary>
        /// Lấy danh sách shop mà tài khoản đang đăng nhập là thành viên
        /// </summary>
        /// <returns>Danh sách shop</returns>
        [HttpGet("my-shops")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ShopDto>>> GetMyShops()
        {
            var accountId = _currentUserService.GetUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            var shops = await _shopManagementService.GetShopsByAccountIdAsync(accountId);
            return Ok(shops);
        }

        #endregion

        #region Seller Endpoints

        /// <summary>
        /// Đăng ký shop mới
        /// </summary>
        /// <param name="createShopDto">Thông tin shop mới</param>
        /// <returns>Shop đã tạo</returns>
        [HttpPost]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ShopDto>> CreateShop(CreateShopDto createShopDto)
        {
            var accountId = _currentUserService.GetUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            try
            {
                // Kiểm tra tên shop đã tồn tại chưa
                var isNameUnique = await _shopManagementService.IsShopNameUniqueAsync(createShopDto.ShopName);
                if (!isNameUnique)
                    return BadRequest(new { error = "Tên shop đã tồn tại" });

                var shop = await _shopManagementService.CreateShopAsync(createShopDto, accountId);
                return CreatedAtAction(nameof(GetShopById), new { id = shop.Id }, shop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo shop mới: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin shop
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <param name="updateShopDto">Thông tin cập nhật</param>
        /// <returns>Shop đã cập nhật</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShopDto>> UpdateShop(Guid id, [FromBody] UpdateShopDto updateShopDto)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            // Kiểm tra người dùng có quyền với shop không
            if (!(await HasShopPermission(id, accountId)))
                return Forbid();

            try
            {
                var command = new UpdateShopCommand
                {
                    Id = id,
                    ShopName = updateShopDto.ShopName,
                    Description = updateShopDto.Description,
                    LogoURL = updateShopDto.LogoURL,
                    CoverImageURL = updateShopDto.CoverImageURL,
                    UpdatedBy = accountId.ToString()
                };

                var shop = await _shopManagementService.UpdateShopAsync(command);
                if (shop == null)
                    return NotFound();

                return Ok(shop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật shop {ShopId}: {Message}", id, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin ngân hàng của shop
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <param name="bankingInfo">Thông tin ngân hàng mới</param>
        /// <returns>Shop đã cập nhật</returns>
        [HttpPut("{id}/banking-info")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShopDto>> UpdateBankingInfo(Guid id, [FromBody] UpdateBankingInfoDto bankingInfo)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            // Kiểm tra người dùng có quyền với shop không
            if (!(await HasShopPermission(id, accountId)))
                return Forbid();

            try
            {
                var shop = await _shopManagementService.UpdateShopBankingInfoAsync(id, bankingInfo, accountId);
                if (shop == null)
                    return NotFound();

                return Ok(shop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin ngân hàng của shop {ShopId}: {Message}", id, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa shop
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <returns>Kết quả thực hiện</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteShop(Guid id)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            // Kiểm tra người dùng có quyền với shop không
            if (!(await HasShopPermission(id, accountId)))
                return Forbid();

            var result = await _shopManagementService.DeleteShopAsync(id, accountId);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Cập nhật trạng thái shop (kích hoạt/vô hiệu hóa)
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <param name="active">true để kích hoạt, false để vô hiệu hóa</param>
        /// <returns>Shop đã cập nhật</returns>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShopDto>> UpdateStatus(Guid id, [FromQuery] bool active)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            // Kiểm tra người dùng có quyền với shop không
            if (!(await HasShopPermission(id, accountId)))
                return Forbid();

            var shop = await _shopManagementService.UpdateShopStatusAsync(id, active, accountId);
            if (shop == null)
                return NotFound();

            return Ok(shop);
        }

        #endregion

        #region Admin Only Endpoints

        /// <summary>
        /// Phê duyệt shop
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <returns>Shop đã phê duyệt</returns>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,ITAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShopDto>> ApproveShop(Guid id)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            var shop = await _shopManagementService.ApproveShopAsync(id, accountId);
            if (shop == null)
                return NotFound();

            return Ok(shop);
        }

        /// <summary>
        /// Từ chối shop
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <param name="reason">Lý do từ chối</param>
        /// <returns>Shop đã từ chối</returns>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin,ITAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShopDto>> RejectShop(Guid id, [FromBody] RejectShopDto rejectDto)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            var shop = await _shopManagementService.RejectShopAsync(id, rejectDto.Reason, accountId);
            if (shop == null)
                return NotFound();

            return Ok(shop);
        }

        /// <summary>
        /// Lấy danh sách shop theo trạng thái phê duyệt
        /// </summary>
        /// <param name="status">Trạng thái phê duyệt</param>
        /// <returns>Danh sách shop</returns>
        [HttpGet("by-approval-status/{status}")]
        [Authorize(Roles = "Admin,ITAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ShopDto>>> GetShopsByApprovalStatus(string status)
        {
            if (!Enum.TryParse<ApprovalStatus>(status, true, out var approvalStatus))
                return BadRequest(new { error = "Trạng thái phê duyệt không hợp lệ" });

            var shops = await _shopManagementService.GetShopsByApprovalStatusAsync(approvalStatus);
            return Ok(shops);
        }

        #endregion

        #region Shop Member Management

        /// <summary>
        /// Lấy danh sách thành viên của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <returns>Danh sách thành viên</returns>
        [HttpGet("{shopId}/members")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ShopMemberDto>>> GetShopMembers(Guid shopId)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            // Kiểm tra người dùng có quyền với shop không
            if (!(await HasShopPermission(shopId, accountId)))
                return Forbid();

            var members = await _shopManagementService.GetShopMembersAsync(shopId);
            return Ok(members);
        }

        /// <summary>
        /// Thêm thành viên vào shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="memberDto">Thông tin thành viên mới</param>
        /// <returns>Kết quả thực hiện</returns>
        [HttpPost("{shopId}/members")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddShopMember(Guid shopId, [FromBody] AddMemberDto memberDto)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            // Kiểm tra người dùng có quyền với shop không
            if (!(await HasShopPermission(shopId, accountId)))
                return Forbid();

            var result = await _shopManagementService.AddShopMemberAsync(shopId, memberDto.AccountId, memberDto.Role, accountId);
            if (!result)
                return BadRequest(new { error = "Không thể thêm thành viên" });

            return Ok(new { success = true });
        }

        /// <summary>
        /// Xóa thành viên khỏi shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="memberId">ID của thành viên</param>
        /// <returns>Kết quả thực hiện</returns>
        [HttpDelete("{shopId}/members/{memberId}")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveShopMember(Guid shopId, Guid memberId)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            // Kiểm tra người dùng có quyền với shop không
            if (!(await HasShopPermission(shopId, accountId)))
                return Forbid();

            var result = await _shopManagementService.RemoveShopMemberAsync(shopId, memberId, accountId);
            if (!result)
                return BadRequest(new { error = "Không thể xóa thành viên" });

            return NoContent();
        }

        /// <summary>
        /// Thay đổi vai trò của thành viên trong shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="memberId">ID của thành viên</param>
        /// <param name="roleDto">Vai trò mới</param>
        /// <returns>Kết quả thực hiện</returns>
        [HttpPut("{shopId}/members/{memberId}/role")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ChangeShopMemberRole(Guid shopId, Guid memberId, [FromBody] ChangeMemberRoleDto roleDto)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            // Kiểm tra người dùng có quyền với shop không
            if (!(await HasShopPermission(shopId, accountId)))
                return Forbid();

            var result = await _shopManagementService.ChangeShopMemberRoleAsync(shopId, memberId, roleDto.Role, accountId);
            if (!result)
                return BadRequest(new { error = "Không thể thay đổi vai trò của thành viên" });

            return Ok(new { success = true });
        }

        #endregion

        #region Invitation Management

        /// <summary>
        /// Gửi lời mời tham gia shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="invitationDto">Thông tin lời mời</param>
        /// <returns>ID của lời mời</returns>
        [HttpPost("{shopId}/invite")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Guid>> InviteToShop(Guid shopId, [FromBody] SendInvitationDto invitationDto)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            // Kiểm tra người dùng có quyền với shop không
            if (!(await HasShopPermission(shopId, accountId)))
                return Forbid();

            var invitationId = await _shopManagementService.SendInvitationAsync(shopId, invitationDto.Email, invitationDto.Role, accountId);
            if (invitationId == Guid.Empty)
                return BadRequest(new { error = "Không thể gửi lời mời" });

            return Ok(new { invitationId });
        }

        /// <summary>
        /// Chấp nhận lời mời tham gia shop
        /// </summary>
        /// <param name="token">Token lời mời</param>
        /// <returns>Kết quả thực hiện</returns>
        [HttpPost("invitations/accept")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AcceptInvitation([FromQuery] string token)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            var result = await _shopManagementService.AcceptInvitationAsync(token, accountId);
            if (!result)
                return BadRequest(new { error = "Không thể chấp nhận lời mời" });

            return Ok(new { success = true });
        }

        /// <summary>
        /// Từ chối lời mời tham gia shop
        /// </summary>
        /// <param name="token">Token lời mời</param>
        /// <returns>Kết quả thực hiện</returns>
        [HttpPost("invitations/decline")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeclineInvitation([FromQuery] string token)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            var result = await _shopManagementService.DeclineInvitationAsync(token, accountId);
            if (!result)
                return BadRequest(new { error = "Không thể từ chối lời mời" });

            return Ok(new { success = true });
        }

        #endregion

        #region Helper Methods

        private Guid GetCurrentUserId()
        {

            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Guid.Empty;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Guid.Empty;

            return userId;
        }

        private async Task<bool> HasShopPermission(Guid shopId, Guid accountId)
        {
            // Admin luôn có quyền
            if (User.IsInRole("Admin") || User.IsInRole("ITAdmin"))
                return true;

            // Kiểm tra quyền với shop
            return await _shopManagementService.HasShopPermissionAsync(shopId, accountId, "Owner");
        }

        #endregion
        [HttpPut("{id}/product-count")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShopDto>> UpdateProductCount(
                  Guid id, [FromBody] UpdateProductCountDto countDto)
        {
            var shop = await _shopManagementService.UpdateProductCountAsync(
                id, countDto.TotalProduct, GetCurrentUserId());

            if (shop == null)
                return NotFound();

            return Ok(shop);
        }
        /// <summary>
        /// Cập nhật tỷ lệ hoàn thành đơn hàng của shop
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Shop đã cập nhật</returns>
        [HttpPut("{id}/completion-rate")]
        [Authorize(Roles = "Admin,System")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShopDto>> UpdateCompletionRate(Guid id, [FromBody] UpdateCompletionRateDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var shop = await _shopManagementService.UpdateShopCompletionRateAsync(
                    id,
                    request.RateChange,
                    request.UpdatedByAccountId);

                if (shop == null)
                    return NotFound(new { error = "Shop không tồn tại" });

                return Ok(shop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật tỷ lệ hoàn thành của shop {ShopId}", id);
                return StatusCode(500, new { error = "Đã xảy ra lỗi khi cập nhật tỷ lệ hoàn thành của shop" });
            }
        }
        /// <summary>
        /// Đồng bộ lại số lượng sản phẩm từ Product Service
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <returns>Shop đã cập nhật</returns>
        [HttpPost("{id}/sync-product-count")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShopDto>> SyncProductCount(Guid id)
        {
            var accountId = GetCurrentUserId();
            if (accountId == Guid.Empty)
                return Unauthorized();

            // Kiểm tra người dùng có quyền với shop không
            if (!(await HasShopPermission(id, accountId)))
                return Forbid();

            try
            {
                var shop = await _shopManagementService.SyncProductCountFromProductServiceAsync(id);
                if (shop == null)
                    return NotFound();

                return Ok(shop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đồng bộ số lượng sản phẩm của shop {ShopId}: {Message}", id, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Đồng bộ tất cả shop - chỉ dành cho Admin
        /// </summary>
        /// <returns>Kết quả thực hiện</returns>
        [HttpPost("sync-all-product-counts")]
        [Authorize(Roles = "Admin,ITAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SyncAllProductCounts()
        {
            try
            {
                var allShops = await _shopManagementService.GetAllShopsAsync();
                var syncResults = new List<object>();

                foreach (var shop in allShops)
                {
                    try
                    {
                        var updatedShop = await _shopManagementService.SyncProductCountFromProductServiceAsync(shop.Id);
                        syncResults.Add(new
                        {
                            ShopId = shop.Id,
                            ShopName = shop.ShopName,
                            Success = updatedShop != null,
                            NewProductCount = updatedShop?.TotalProduct ?? 0
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi đồng bộ shop {ShopId}", shop.Id);
                        syncResults.Add(new
                        {
                            ShopId = shop.Id,
                            ShopName = shop.ShopName,
                            Success = false,
                            Error = ex.Message
                        });
                    }
                }

                return Ok(new { Results = syncResults });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đồng bộ tất cả shop");
                return BadRequest(new { error = ex.Message });
            }
        }
    }   
}