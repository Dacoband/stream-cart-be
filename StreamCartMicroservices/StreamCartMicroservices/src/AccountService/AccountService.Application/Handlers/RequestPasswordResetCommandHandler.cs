using AccountService.Application.Commands;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using Shared.Common.Services.Email;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers
{
    public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, bool>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;

        public RequestPasswordResetCommandHandler(
            IAccountRepository accountRepository,
            IEmailService emailService)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<bool> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId.ToString());
            
            if (account == null)
            {
                return false;
            }

            string resetToken = GenerateResetToken();

            account.SetVerificationToken(resetToken);
            account.SetVerificationTokenExpiry(DateTime.UtcNow.AddHours(24));
            await _accountRepository.ReplaceAsync(account.Id.ToString(), account);

            // Gửi email với token
            var resetLink = $"https://yourwebsite.com/reset-password?id={account.Id}&token={resetToken}";
            var htmlBody = $@"
                <h2>Reset Your Password</h2>
                <p>Hello {account.Fullname ?? account.Username},</p>
                <p>You've requested to reset your password. Click the link below to set a new password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you didn't request this, you can ignore this email.</p>
                <p>The link will expire in 24 hours.</p>
                <p>Thank you,<br/>Stream Cart Team</p>
            ";
            
            await _emailService.SendEmailAsync(account.Email, "Password Reset Request", htmlBody, account.Fullname);
            
            return true;
        }

        private string GenerateResetToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            
            return Convert.ToBase64String(randomBytes);
        }
    }
}