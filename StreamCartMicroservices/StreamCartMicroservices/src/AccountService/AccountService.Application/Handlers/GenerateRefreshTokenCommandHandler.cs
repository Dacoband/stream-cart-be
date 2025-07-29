using AccountService.Application.Commands;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers
{
    public class GenerateRefreshTokenCommandHandler : IRequestHandler<GenerateRefreshTokenCommand, string>
    {
        private readonly IAccountRepository _accountRepository;

        public GenerateRefreshTokenCommandHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<string> Handle(GenerateRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Lấy tài khoản từ ID
            var account = await _accountRepository.GetByIdAsync(request.AccountId.ToString());
            
            if (account == null)
            {
                throw new ApplicationException($"Account with ID {request.AccountId} not found");
            }

            // Tạo refresh token
            var refreshToken = GenerateRandomToken();

            // Lưu refresh token vào database với thời hạn 7 ngày
            account.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
            await _accountRepository.ReplaceAsync(account.Id.ToString(), account);

            return refreshToken;
        }

        private string GenerateRandomToken()
        {
            // Tạo một token ngẫu nhiên 32 byte (256 bit)
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            
            // Chuyển token thành chuỗi base64 để dễ lưu trữ và truyền tải
            return Convert.ToBase64String(randomBytes);
        }
    }
}