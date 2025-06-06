using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using AccountService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;

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
    }
}