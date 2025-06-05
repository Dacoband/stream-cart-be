using AccountService.Application.DTOs;
using AccountService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AccountService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AccountManagementService _accountService;

        public AuthController(AccountManagementService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid login data"));

            var result = await _accountService.LoginAsync(loginDto);
            
            if (!result.Success)
                return BadRequest(ApiResponse<object>.ErrorResult(result.Message));

            return Ok(ApiResponse<AuthResultDto>.SuccessResult(result, result.Message));
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid password data"));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return BadRequest(ApiResponse<object>.ErrorResult("User identity not found"));

            if (!Guid.TryParse(userIdClaim.Value, out var accountId))
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid user identity"));

            if (string.IsNullOrWhiteSpace(changePasswordDto.NewPassword) ||
                changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("New passwords do not match"));
            }

            var result = await _accountService.ChangePasswordAsync(accountId, changePasswordDto);
            
            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResult("Failed to change password. Current password may be incorrect."));

            return Ok(ApiResponse<bool>.SuccessResult(true, "Password changed successfully"));
        }

        [HttpPost("verify-account/{accountId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> VerifyAccount(Guid accountId, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(ApiResponse<object>.ErrorResult("Verification token is required"));

            var result = await _accountService.VerifyAccountAsync(accountId, token);
            
            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResult("Account verification failed"));

            return Ok(ApiResponse<bool>.SuccessResult(true, "Account verified successfully"));
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(ApiResponse<object>.ErrorResult("Email is required"));

            await _accountService.RequestPasswordResetAsync(email);
            
            // For security reasons, always return success even if the email doesn't exist
            return Ok(ApiResponse<bool>.SuccessResult(true, 
                "If the email is registered, a password reset link has been sent"));
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> ResetPassword([FromQuery] Guid accountId, [FromQuery] string token, [FromBody] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(ApiResponse<object>.ErrorResult("Reset token is required"));

            if (string.IsNullOrWhiteSpace(newPassword))
                return BadRequest(ApiResponse<object>.ErrorResult("New password is required"));

            var result = await _accountService.ResetPasswordAsync(accountId, token, newPassword);
            
            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResult("Password reset failed"));

            return Ok(ApiResponse<bool>.SuccessResult(true, "Password reset successfully"));
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public IActionResult RefreshToken([FromBody] string refreshToken)
        {
            // Note: This endpoint needs to be implemented with a refresh token handler
            // For now, return a placeholder response
            return BadRequest(ApiResponse<object>.ErrorResult("Refresh token functionality not implemented yet"));
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var accountId))
                return BadRequest(ApiResponse<object>.ErrorResult("User identity not found"));

            var account = await _accountService.GetAccountByIdAsync(accountId);
            
            if (account == null)
                return NotFound(ApiResponse<object>.ErrorResult("Current user not found"));

            return Ok(ApiResponse<AccountDto>.SuccessResult(account));
        }
    }
}