using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using AccountService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System.Security.Claims;

namespace AccountService.Api.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountManagementService _accountService;
        private readonly ICurrentUserService _currentUserService;

        public AccountController(IAccountManagementService accountService,ICurrentUserService currentUserService)
        {
            _accountService = accountService;
            _currentUserService = currentUserService;
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountDto updateAccountDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid account data"));

            var command = new UpdateAccountCommand
            {
                Id = id,
                PhoneNumber = updateAccountDto.PhoneNumber,
                Fullname = updateAccountDto.Fullname,
                AvatarURL = updateAccountDto.AvatarURL,
                IsActive = updateAccountDto.IsActive,
                IsVerified = updateAccountDto.IsVerified,
                CompleteRate = updateAccountDto.CompleteRate,
                Role = updateAccountDto.Role,
                ShopId = updateAccountDto.ShopId,
                UpdatedBy = _currentUserService.GetUserId().ToString() 
            };

            var updatedAccount = await _accountService.UpdateAccountAsync(command);

            return Ok(ApiResponse<AccountDto>.SuccessResult(updatedAccount, "Account updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ITAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 403)]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            try
            {
                var accountToDelete = await _accountService.GetAccountByIdAsync(id);
                if (accountToDelete == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Account not found"));
                }

                // Kiểm tra không được xóa ITAdmin
                if (accountToDelete.Role == RoleType.ITAdmin)
                {
                    return Forbid(ApiResponse<object>.ErrorResult("Cannot delete ITAdmin account").ToString());
                }

                // Kiểm tra nếu là OperationManager, phải đảm bảo còn ít nhất 1 OperationManager khác
                if (accountToDelete.Role == RoleType.OperationManager)
                {
                    var operationManagers = await _accountService.GetAccountsByRoleAsync(RoleType.OperationManager);
                    var activeOperationManagers = operationManagers.Where(om => om.IsActive && om.Id != id).Count();

                    if (activeOperationManagers < 1)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("Cannot delete the last active Operation Manager. At least one Operation Manager must exist in the system."));
                    }
                }

                var result = await _accountService.DeleteAccountAsync(id);

                if (!result)
                    return BadRequest(ApiResponse<object>.ErrorResult("Failed to delete account"));

                return Ok(ApiResponse<bool>.SuccessResult(true, "Account deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error deleting account: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetAccountById(Guid id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);

            if (account == null)
                return NotFound(ApiResponse<object>.ErrorResult("Account not found"));

            return Ok(ApiResponse<AccountDto>.SuccessResult(account));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AccountDto>>), 200)]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(ApiResponse<IEnumerable<AccountDto>>.SuccessResult(accounts));
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<AccountDto>>), 200)]
        public async Task<IActionResult> GetAccountsPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagedAccounts = await _accountService.GetAccountsPagedAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<AccountDto>>.SuccessResult(pagedAccounts));
        }

        [HttpGet("by-role/{role}")]
        [Authorize(Roles = "ITAdmin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AccountDto>>), 200)]
        public async Task<IActionResult> GetAccountsByRole(RoleType role)
        {
            var accounts = await _accountService.GetAccountsByRoleAsync(role);
            return Ok(ApiResponse<IEnumerable<AccountDto>>.SuccessResult(accounts));
        }
        [HttpPost("moderators")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        public async Task<IActionResult> CreateModeratorAccount([FromBody] CreateAccountModeratorDto createAccountDto, [FromQuery] Guid shopId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid account data"));

            var userIdClaim = _currentUserService.GetUserId();
       
            try
            {
                if (!await _accountService.IsUsernameUniqueAsync(createAccountDto.Username))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Username already exists"));
                }

                if (!await _accountService.IsEmailUniqueAsync(createAccountDto.Email))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Email already exists"));
                }
                var command = new CreateAccountCommand
                {
                    Username = createAccountDto.Username,
                    Email = createAccountDto.Email,
                    Password = createAccountDto.Password,
                    PhoneNumber = createAccountDto.PhoneNumber,
                    Fullname = createAccountDto.Fullname,
                    AvatarURL = createAccountDto.AvatarURL
                };

                var createdAccount = await _accountService.CreateModeratorAccountAsync(command, shopId, userIdClaim);

                return CreatedAtAction(
                    nameof(GetAccountById),
                    new { id = createdAccount.Id },
                    ApiResponse<AccountDto>.SuccessResult(createdAccount, "Moderator account created successfully")
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error creating moderator account: {ex.Message}"));
            }
        }
        
        [HttpPost("operation-managers")]
        [Authorize(Roles = "ITAdmin")]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        public async Task<IActionResult> CreateOperationManagerAccount([FromBody] CreateAccountDto createAccountDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid account data"));

            var userIdClaim = _currentUserService.GetUserId();
            
            try
            {
                if (!await _accountService.IsUsernameUniqueAsync(createAccountDto.Username))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Username already exists"));
                }

                if (!await _accountService.IsEmailUniqueAsync(createAccountDto.Email))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Email already exists"));
                }

                var command = new CreateAccountCommand
                {
                    Username = createAccountDto.Username,
                    Email = createAccountDto.Email,
                    Password = createAccountDto.Password,
                    PhoneNumber = createAccountDto.PhoneNumber,
                    Fullname = createAccountDto.Fullname,
                    AvatarURL = createAccountDto.AvatarURL
                };

                var createdAccount = await _accountService.CreateOperationManagerAccountAsync(command, userIdClaim);

                return CreatedAtAction(
                    nameof(GetAccountById),
                    new { id = createdAccount.Id },
                    ApiResponse<AccountDto>.SuccessResult(createdAccount, "Operation Manager account created successfully")
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error creating operation manager account: {ex.Message}"));
            }
        }

        //Inactive/Active Account
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Seller,ITAdmin")]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 403)]
        public async Task<IActionResult> UpdateAccountStatus(Guid id, [FromBody] bool isActive)
        {
            // Get the current user's ID
            var userIdClaim = _currentUserService.GetUserId();

            try
            {
                //if (!await _accountService.CanManageAccountStatusAsync(userIdClaim, id))
                //    return Forbid(ApiResponse<object>.ErrorResult("You don't have permission to change this account's status").ToString());

                var updatedAccount = await _accountService.UpdateAccountStatusAsync(id, isActive, userIdClaim);

                string status = isActive ? "activated" : "deactivated";
                return Ok(ApiResponse<AccountDto>.SuccessResult(updatedAccount, $"Account successfully {status}"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ApiResponse<object>.ErrorResult(ex.Message).ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error updating account status: {ex.Message}"));
            }
        }

        // Get Moderators by Shop
        [HttpGet("moderators/by-shop/{shopId}")]
        [Authorize(Roles = "Seller,OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AccountDto>>), 200)]
        public async Task<IActionResult> GetModeratorsByShop(Guid shopId)
        {
            var userIdClaim = _currentUserService.GetUserId();

            try
            {
                //var sellerAccount = await _accountService.GetAccountByIdAsync(userIdClaim);
                //if (sellerAccount == null || sellerAccount.Role != RoleType.Seller || sellerAccount.ShopId != shopId)
                //    return Forbid(ApiResponse<object>.ErrorResult("You don't have permission to access moderators for this shop").ToString());

                var allModerators = await _accountService.GetAccountsByRoleAsync(RoleType.Moderator);

                var shopModerators = allModerators.Where(m => m.ShopId == shopId).ToList();

                return Ok(ApiResponse<IEnumerable<AccountDto>>.SuccessResult(shopModerators, "Moderators retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error retrieving moderators: {ex.Message}"));
            }
        }

        [HttpPut("{id}/shop")]
        [AllowAnonymous] 
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateAccountShopInfo(Guid id, [FromBody] UpdateShopInfoDto dto)
        {
            try
            {
                var command = new UpdateAccountCommand
                {
                    Id = id,
                    ShopId = dto.ShopId,
                    UpdatedBy = "ShopService" 
                };

                var updatedAccount = await _accountService.UpdateAccountAsync(command);
                return Ok(ApiResponse<AccountDto>.SuccessResult(updatedAccount, "Account ShopId updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error updating ShopId: {ex.Message}"));
            }
        }

        [HttpGet("by-shop/{shopId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ShopAccountDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAccountByShopId(Guid shopId)
        {
            try
            {
                var shopAccounts = await _accountService.GetAccountsByShopIdAsync(shopId);

                if (shopAccounts?.Any() != true)
                    return NotFound(ApiResponse<object>.ErrorResult($"No accounts found for shop with ID: {shopId}"));

                var result = shopAccounts.Select(a => new ShopAccountDto
                {
                    Id = a.Id,
                    Fullname = a.Fullname ?? string.Empty,
                    Role = a.Role
                }).ToList();

                return Ok(ApiResponse<IEnumerable<ShopAccountDto>>.SuccessResult(result, "Accounts retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ApiResponse<object>.ErrorResult(ex.Message).ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.ErrorResult($"Error retrieving accounts: {ex.Message}"));
            }
        }
        /// <summary>
        /// Xóa moderator khỏi shop (vô hiệu hóa tài khoản moderator)
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="moderatorId">ID của moderator cần xóa</param>
        /// <returns>Kết quả thực hiện</returns>
        [HttpDelete("moderators/{moderatorId}/shops/{shopId}")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 403)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> RemoveModeratorFromShop(Guid shopId, Guid moderatorId)
        {
            var userIdClaim = _currentUserService.GetUserId();

            try
            {
                // Kiểm tra người thực hiện có phải seller của shop này không
                var sellerAccount = await _accountService.GetAccountByIdAsync(userIdClaim);
                if (sellerAccount == null || sellerAccount.Role != RoleType.Seller || sellerAccount.ShopId != shopId)
                {
                    return Forbid(ApiResponse<object>.ErrorResult("Bạn không có quyền xóa moderator khỏi shop này").ToString());
                }

                // Kiểm tra moderator có tồn tại không
                var moderatorAccount = await _accountService.GetAccountByIdAsync(moderatorId);
                if (moderatorAccount == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy moderator"));
                }

                // Kiểm tra moderator có thuộc shop này không
                if (moderatorAccount.Role != RoleType.Moderator || moderatorAccount.ShopId != shopId)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Moderator không thuộc shop này"));
                }

                // Vô hiệu hóa tài khoản moderator và gỡ khỏi shop
                var result = await _accountService.RemoveModeratorFromShopAsync(moderatorId, shopId, userIdClaim);

                if (!result)
                    return BadRequest(ApiResponse<object>.ErrorResult("Không thể xóa moderator khỏi shop"));

                return Ok(ApiResponse<bool>.SuccessResult(true, "Đã xóa moderator khỏi shop thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ApiResponse<object>.ErrorResult(ex.Message).ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi khi xóa moderator: {ex.Message}"));
            }
        }

        /// <summary>
        /// Vô hiệu hóa tài khoản moderator (soft delete)
        /// </summary>
        /// <param name="moderatorId">ID của moderator cần vô hiệu hóa</param>
        /// <param name="request">Thông tin vô hiệu hóa</param>
        /// <returns>Kết quả thực hiện</returns>
        [HttpPost("moderators/{moderatorId}/deactivate")]
        [Authorize(Roles = "Seller,OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 403)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> DeactivateModerator(Guid moderatorId, [FromBody] DeactivateModeratorDto request)
        {
            var userIdClaim = _currentUserService.GetUserId();

            try
            {
                // Kiểm tra moderator có tồn tại không
                var moderatorAccount = await _accountService.GetAccountByIdAsync(moderatorId);
                if (moderatorAccount == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy moderator"));
                }

                if (moderatorAccount.Role != RoleType.Moderator)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Tài khoản này không phải là moderator"));
                }

                // Kiểm tra quyền: Seller chỉ có thể vô hiệu hóa moderator của shop mình
                if (User.IsInRole("Seller"))
                {
                    var sellerAccount = await _accountService.GetAccountByIdAsync(userIdClaim);
                    if (sellerAccount?.ShopId != moderatorAccount.ShopId)
                    {
                        return Forbid(ApiResponse<object>.ErrorResult("Bạn không có quyền vô hiệu hóa moderator này").ToString());
                    }
                }

                // Vô hiệu hóa moderator
                var updatedAccount = await _accountService.UpdateAccountStatusAsync(moderatorId, false, userIdClaim);

                return Ok(ApiResponse<AccountDto>.SuccessResult(updatedAccount,
                    $"Đã vô hiệu hóa moderator thành công. Lý do: {request.Reason}"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ApiResponse<object>.ErrorResult(ex.Message).ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi khi vô hiệu hóa moderator: {ex.Message}"));
            }
        }

        /// <summary>
        /// Kích hoạt lại tài khoản moderator
        /// </summary>
        /// <param name="moderatorId">ID của moderator cần kích hoạt</param>
        /// <returns>Kết quả thực hiện</returns>
        [HttpPost("moderators/{moderatorId}/reactivate")]
        [Authorize(Roles = "Seller,OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 403)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> ReactivateModerator(Guid moderatorId)
        {
            var userIdClaim = _currentUserService.GetUserId();

            try
            {
                // Kiểm tra moderator có tồn tại không
                var moderatorAccount = await _accountService.GetAccountByIdAsync(moderatorId);
                if (moderatorAccount == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy moderator"));
                }

                if (moderatorAccount.Role != RoleType.Moderator)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Tài khoản này không phải là moderator"));
                }

                // Kiểm tra quyền: Seller chỉ có thể kích hoạt moderator của shop mình
                if (User.IsInRole("Seller"))
                {
                    var sellerAccount = await _accountService.GetAccountByIdAsync(userIdClaim);
                    if (sellerAccount?.ShopId != moderatorAccount.ShopId)
                    {
                        return Forbid(ApiResponse<object>.ErrorResult("Bạn không có quyền kích hoạt moderator này").ToString());
                    }
                }

                // Kích hoạt lại moderator
                var updatedAccount = await _accountService.UpdateAccountStatusAsync(moderatorId, true, userIdClaim);

                return Ok(ApiResponse<AccountDto>.SuccessResult(updatedAccount, "Đã kích hoạt lại moderator thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ApiResponse<object>.ErrorResult(ex.Message).ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi khi kích hoạt moderator: {ex.Message}"));
            }
        }
    }
    public class UpdateShopInfoDto
    {
        public Guid ShopId { get; set; }
    }

    public class ShopAccountDto
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public RoleType Role { get; set; }
    }
}