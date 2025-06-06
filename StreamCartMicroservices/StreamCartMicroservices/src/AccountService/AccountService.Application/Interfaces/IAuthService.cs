using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace AccountService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AccountDto> RegisterAsync(CreateAccountCommand command);
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task<bool> ChangePasswordAsync(Guid accountId, ChangePasswordDto changePasswordDto);
        Task<bool> VerifyAccountAsync(Guid accountId, string verificationToken);
        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(Guid accountId, string token, string newPassword);
        Task<AccountDto> GetCurrentUserAsync(Guid accountId);
    }
}