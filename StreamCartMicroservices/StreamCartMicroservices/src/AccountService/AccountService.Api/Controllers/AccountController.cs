using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using AccountService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using System.Security.Claims;

namespace AccountService.Api.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountManagementService _accountService;

        public AccountController(IAccountManagementService accountService)
        {
            _accountService = accountService;
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
            };

            var updatedAccount = await _accountService.UpdateAccountAsync(command);

            return Ok(ApiResponse<AccountDto>.SuccessResult(updatedAccount, "Account updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            var result = await _accountService.DeleteAccountAsync(id);

            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResult("Failed to delete account"));

            return Ok(ApiResponse<bool>.SuccessResult(true, "Account deleted successfully"));
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
        [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> CreateModeratorAccount([FromBody] CreateAccountDto createAccountDto, [FromQuery] Guid shopId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid account data"));

            // Get the seller's account ID from the claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var sellerAccountId))
                return BadRequest(ApiResponse<object>.ErrorResult("User identity not found"));

            try
            {
                // Check username and email uniqueness
                if (!await _accountService.IsUsernameUniqueAsync(createAccountDto.Username))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Username already exists"));
                }

                if (!await _accountService.IsEmailUniqueAsync(createAccountDto.Email))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Email already exists"));
                }

                // Create command
                var command = new CreateAccountCommand
                {
                    Username = createAccountDto.Username,
                    Email = createAccountDto.Email,
                    Password = createAccountDto.Password,
                    PhoneNumber = createAccountDto.PhoneNumber,
                    Fullname = createAccountDto.Fullname,
                    AvatarURL = createAccountDto.AvatarURL
                };

                // Create moderator account
                var createdAccount = await _accountService.CreateModeratorAccountAsync(command, shopId, sellerAccountId);

                // Return success response
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

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var itAdminAccountId))
                return BadRequest(ApiResponse<object>.ErrorResult("User identity not found"));
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

                var createdAccount = await _accountService.CreateOperationManagerAccountAsync(command, itAdminAccountId);

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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResult("User identity not found"));
            try
            {
                if (!await _accountService.CanManageAccountStatusAsync(userId, id))
                    return Forbid(ApiResponse<object>.ErrorResult("You don't have permission to change this account's status").ToString());

                var updatedAccount = await _accountService.UpdateAccountStatusAsync(id, isActive, userId);

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
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AccountDto>>), 200)]
        public async Task<IActionResult> GetModeratorsByShop(Guid shopId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResult("User identity not found"));
            try
            {
                var sellerAccount = await _accountService.GetAccountByIdAsync(userId);
                if (sellerAccount == null || sellerAccount.Role != RoleType.Seller || sellerAccount.ShopId != shopId)
                    return Forbid(ApiResponse<object>.ErrorResult("You don't have permission to access moderators for this shop").ToString());

                var allModerators = await _accountService.GetAccountsByRoleAsync(RoleType.Moderator);

                var shopModerators = allModerators.Where(m => m.ShopId == shopId).ToList();

                return Ok(ApiResponse<IEnumerable<AccountDto>>.SuccessResult(shopModerators, "Moderators retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error retrieving moderators: {ex.Message}"));
            }
        }
    }
}