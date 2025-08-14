using AccountService.Application.Commands;
using AccountService.Application.DTOs;
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
using System.Threading;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMediator _mediator;
        private readonly JwtSettings _jwtSettings;

        public RefreshTokenCommandHandler(
            IAccountRepository accountRepository,
            IMediator mediator,
            IOptions<JwtSettings> jwtSettings)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        }

        public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Tìm account có refresh token này
                var account = await _accountRepository.GetByRefreshTokenAsync(request.RefreshToken);
                
                if (account == null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Invalid refresh token"
                    };
                }

                // Kiểm tra refresh token có hợp lệ không
                if (!account.IsRefreshTokenValid(request.RefreshToken))
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Refresh token has expired"
                    };
                }

                // Kiểm tra account có active không
                if (!account.IsActive)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Account is disabled"
                    };
                }

                // Tạo access token mới
                var newAccessToken = GenerateJwtToken(account);

                // Tạo refresh token mới
                var newRefreshToken = await _mediator.Send(new GenerateRefreshTokenCommand
                {
                    AccountId = account.Id
                }, cancellationToken);

                // Lưu refresh token mới vào database
                account.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
                await _accountRepository.ReplaceAsync(account.Id.ToString(), account);

                return new AuthResultDto
                {
                    Success = true,
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    Account = MapToAccountDto(account),
                    Message = "Token refreshed successfully"
                };
            }
            catch (Exception ex)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = $"Error refreshing token: {ex.Message}"
                };
            }
        }

        private string GenerateJwtToken(Domain.Entities.Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            var now = DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new Claim("id", account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role.ToString()),
                new Claim("ShopId", account.ShopId.ToString())
            };
            var expiryTime = now.AddMinutes(_jwtSettings.ExpiryMinutes + 5); // Thêm 5 phút buffer

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                IssuedAt = now,
                Expires = expiryTime,
                NotBefore = now.AddSeconds(-30), // ✅ Cho phép token hoạt động ngay lập tức
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private AccountDto MapToAccountDto(Domain.Entities.Account account)
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