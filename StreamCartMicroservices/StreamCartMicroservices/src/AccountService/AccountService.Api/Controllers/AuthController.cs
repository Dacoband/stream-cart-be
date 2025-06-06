using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Models;
using System.Security.Claims;

namespace AccountService.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAccountManagementService _accountService;

        public AuthController(IAuthService authService, IAccountManagementService accountService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
                return BadRequest(ApiResponse<object>.ErrorResult("Username and password are required"));

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
                return BadRequest(ApiResponse<object>.ErrorResult(result.Message));

            return Ok(ApiResponse<AuthResultDto>.SuccessResult(result));
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] CreateAccountDto createAccountDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid account data"));

            // Kiểm tra username và email đã tồn tại chưa
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

            var createdAccount = await _authService.RegisterAsync(command);

            return CreatedAtAction(
                "GetAccountById",       
                "Account",        
                new { id = createdAccount.Id },
                ApiResponse<AccountDto>.SuccessResult(createdAccount, "Account registered successfully")
            );
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var accountId))
                return BadRequest(ApiResponse<object>.ErrorResult("User identity not found"));

            var result = await _authService.ChangePasswordAsync(accountId, changePasswordDto);

            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResult("Password change failed"));

            return Ok(ApiResponse<bool>.SuccessResult(true, "Password changed successfully"));
        }

        [HttpGet("verify")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> VerifyAccount([FromQuery] Guid id, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(ApiResponse<object>.ErrorResult("Verification token is required"));

            var result = await _authService.VerifyAccountAsync(id, token);

            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResult("Account verification failed"));

            return Ok(ApiResponse<bool>.SuccessResult(true, "Account verified successfully"));
        }

        [HttpPost("reset-password-request")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> RequestPasswordReset([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(ApiResponse<object>.ErrorResult("Email is required"));

            var result = await _authService.RequestPasswordResetAsync(email);

            return Ok(ApiResponse<bool>.SuccessResult(true, "If the email exists, a password reset link has been sent"));
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

            var result = await _authService.ResetPasswordAsync(accountId, token, newPassword);

            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResult("Password reset failed"));

            return Ok(ApiResponse<bool>.SuccessResult(true, "Password reset successfully"));
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public IActionResult RefreshToken([FromBody] string refreshToken)
        {
            return BadRequest(ApiResponse<object>.ErrorResult("Refresh token functionality not implemented yet"));
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var accountId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User identity not found"));

                var account = await _authService.GetCurrentUserAsync(accountId);

                if (account == null)
                    return NotFound(ApiResponse<object>.ErrorResult("Current user not found"));

                return Ok(ApiResponse<AccountDto>.SuccessResult(account));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
    }
}
