using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Application.Services;
using AccountService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AccountService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AccountManagementService _accountService;
        
        public AccountController(AccountManagementService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            
            if (account == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Account with ID {id} not found."));
            
            return Ok(ApiResponse<AccountDto>.SuccessResult(account));
        }

        [HttpGet("username/{username}")]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var account = await _accountService.GetAccountByUsernameAsync(username);
            
            if (account == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Account with username {username} not found."));
            
            return Ok(ApiResponse<AccountDto>.SuccessResult(account));
        }

        [HttpGet("email/{email}")]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var account = await _accountService.GetAccountByEmailAsync(email);
            
            if (account == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Account with email {email} not found."));
            
            return Ok(ApiResponse<AccountDto>.SuccessResult(account));
        }

        [HttpGet]
        //[Authorize(Roles = "Admin,ITAdmin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AccountDto>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(ApiResponse<IEnumerable<AccountDto>>.SuccessResult(accounts));
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin,ITAdmin")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<AccountDto>>), 200)]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagedAccounts = await _accountService.GetAccountsPagedAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<AccountDto>>.SuccessResult(pagedAccounts));
        }

        [HttpGet("by-role/{role}")]
        [Authorize(Roles = "Admin,ITAdmin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AccountDto>>), 200)]
        public async Task<IActionResult> GetByRole(RoleType role)
        {
            var accounts = await _accountService.GetAccountsByRoleAsync(role);
            return Ok(ApiResponse<IEnumerable<AccountDto>>.SuccessResult(accounts));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Create([FromBody] CreateAccountDto createAccountDto)
        {
            try
            {
                // Check if username and email are unique
                if (!await _accountService.IsUsernameUniqueAsync(createAccountDto.Username))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Username is already taken"));
                }

                if (!await _accountService.IsEmailUniqueAsync(createAccountDto.Email))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Email is already registered"));
                }

                var command = new CreateAccountCommand
                {
                    Username = createAccountDto.Username,
                    Email = createAccountDto.Email,
                    Password = createAccountDto.Password,
                    PhoneNumber = createAccountDto.PhoneNumber,
                    Fullname = createAccountDto.Fullname,
                    AvatarURL = createAccountDto.AvatarURL,
                    Role = createAccountDto.Role,
                    IsActive = createAccountDto.IsActive,
                    IsVerified = createAccountDto.IsVerified
                };

                var result = await _accountService.CreateAccountAsync(command);
                
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<AccountDto>.SuccessResult(result, "Account created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error creating account: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountDto updateAccountDto)
        {
            try
            {
                // First check if account exists
                var existingAccount = await _accountService.GetAccountByIdAsync(id);
                if (existingAccount == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Account with ID {id} not found"));

                // Authorization check - only allow admins or the account owner to update
                if (!User.IsInRole("Admin") && !User.IsInRole("ITAdmin"))
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null || Guid.Parse(userIdClaim.Value) != id)
                    {
                        return Forbid();
                    }
                }

                var command = new UpdateAccountCommand
                {
                    Id = id,
                    PhoneNumber = updateAccountDto.PhoneNumber,
                    Fullname = updateAccountDto.Fullname,
                    AvatarURL = updateAccountDto.AvatarURL,
                    Role = updateAccountDto.Role,
                    IsActive = updateAccountDto.IsActive,
                    IsVerified = updateAccountDto.IsVerified,
                    CompleteRate = updateAccountDto.CompleteRate,
                    ShopId = updateAccountDto.ShopId,
                    UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system"
                };

                var result = await _accountService.UpdateAccountAsync(command);
                
                return Ok(ApiResponse<AccountDto>.SuccessResult(result, "Account updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating account: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,ITAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // First check if account exists
                var existingAccount = await _accountService.GetAccountByIdAsync(id);
                if (existingAccount == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Account with ID {id} not found"));

                var result = await _accountService.DeleteAccountAsync(id);
                
                if (result)
                    return Ok(ApiResponse<bool>.SuccessResult(true, "Account deleted successfully"));
                else
                    return BadRequest(ApiResponse<object>.ErrorResult("Failed to delete account"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting account: {ex.Message}"));
            }
        }

        [HttpGet("check-username/{username}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> CheckUsernameAvailability(string username)
        {
            var isUnique = await _accountService.IsUsernameUniqueAsync(username);
            return Ok(ApiResponse<bool>.SuccessResult(isUnique));
        }

        [HttpGet("check-email/{email}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> CheckEmailAvailability(string email)
        {
            var isUnique = await _accountService.IsEmailUniqueAsync(email);
            return Ok(ApiResponse<bool>.SuccessResult(isUnique));
        }
    }
}