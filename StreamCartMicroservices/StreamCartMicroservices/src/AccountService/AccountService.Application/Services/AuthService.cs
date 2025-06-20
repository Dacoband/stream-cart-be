using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using AccountService.Domain.Entities;
using AccountService.Domain.Enums;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Common.Settings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMediator _mediator;
        private readonly IAccountRepository _accountRepository;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IMediator mediator,
            IAccountRepository accountRepository,
            IOptions<JwtSettings> jwtSettings)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        }

        public async Task<AccountDto> RegisterAsync(CreateAccountCommand command)
        {
            if (command.Role != RoleType.Customer && command.Role != RoleType.Seller)
            {
                command.Role = RoleType.Customer;  
            }

            command.IsActive = true;
            command.IsVerified = false;

            return await _mediator.Send(command);
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            // First, find the account by username
            var account = await _accountRepository.GetByUsernameAsync(loginDto.Username);
            
            if (account == null)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Username or password is incorrect"
                };
            }
            // Verify the password
            bool isPasswordValid = await _mediator.Send(new VerifyPasswordCommand 
            { 
                AccountId = account.Id,
                Password = loginDto.Password 
            });

            if (!isPasswordValid)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Username or password is incorrect"
                };
            }
            // Check if account is active
            if (!account.IsActive)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Account is disabled"
                };
            }
            if (!account.IsVerified)
            {
                // Gửi lại OTP xác thực qua email
                await _mediator.Send(new GenerateOTPCommand
                {
                    AccountId = account.Id,
                    Email = account.Email
                });

                return new AuthResultDto
                {
                    Success = false,
                    RequiresVerification = true,
                    Account = MapToAccountDto(account),
                    Message = "Account requires verification. A new OTP has been sent to your email."
                };
            }
            // Update last login date
            await _mediator.Send(new UpdateLastLoginCommand { AccountId = account.Id });

            // Generate JWT token
            var token = GenerateJwtToken(account);
            
            // Generate refresh token
            var refreshToken = await _mediator.Send(new GenerateRefreshTokenCommand 
            { 
                AccountId = account.Id
            });

            return new AuthResultDto
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                Account = MapToAccountDto(account),
                Message = "Login successful"
            };
        }

        public async Task<bool> ChangePasswordAsync(Guid accountId, ChangePasswordDto changePasswordDto)
        {
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
            {
                return false;
            }

            // First verify the current password
            bool isCurrentPasswordValid = await _mediator.Send(new VerifyPasswordCommand
            {
                AccountId = accountId,
                Password = changePasswordDto.CurrentPassword
            });

            if (!isCurrentPasswordValid)
            {
                return false;
            }

            // Change password using mediator
            return await _mediator.Send(new ChangePasswordCommand
            {
                AccountId = accountId,
                NewPassword = changePasswordDto.NewPassword
            });
        }

        public async Task<bool> VerifyAccountAsync(Guid accountId, string verificationToken)
        {
            return await _mediator.Send(new VerifyAccountCommand
            {
                AccountId = accountId,
                VerificationToken = verificationToken
            });
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            var account = await _accountRepository.GetByEmailAsync(email);
            
            if (account == null)
            {
                return true;
            }

            return await _mediator.Send(new RequestPasswordResetCommand
            {
                AccountId = account.Id,
                Email = email
            });
        }

        public async Task<bool> ResetPasswordAsync(Guid accountId, string token, string newPassword)
        {
            return await _mediator.Send(new ResetPasswordCommand
            {
                AccountId = accountId,
                ResetToken = token,
                NewPassword = newPassword
            });
        }

        public async Task<AccountDto> GetCurrentUserAsync(Guid accountId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId.ToString());
            return MapToAccountDto(account);
        }

        private string GenerateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        private AccountDto MapToAccountDto(Account account)
        {
            if (account == null)
                return null;

            return new AccountDto
            {
                Id = account.Id,
                Username = account.Username,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                Fullname = account.Fullname,
                AvatarURL = account.AvatarURL,
                Role = account.Role,
                RegistrationDate = account.RegistrationDate,
                LastLoginDate = account.LastLoginDate,
                IsActive = account.IsActive,
                IsVerified = account.IsVerified,
                CompleteRate = account.CompleteRate,
                ShopId = account.ShopId,
                CreatedAt = account.CreatedAt,
                CreatedBy = account.CreatedBy,
                LastModifiedAt = account.LastModifiedAt,
                LastModifiedBy = account.LastModifiedBy
            };
        }
    }
}
