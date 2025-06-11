using AccountService.Application.Commands;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using Shared.Common.Services.Email;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers
{
    public class VerifyAccountCommandHandler : IRequestHandler<VerifyAccountCommand, bool>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;

        public VerifyAccountCommandHandler(
            IAccountRepository accountRepository,
            IEmailService emailService)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<bool> Handle(VerifyAccountCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId.ToString());
            
            if (account == null)
            {
                return false;
            }

            try
            {
                if (account.VerificationToken != request.VerificationToken)
                {
                    return false; 
                }

                if (!account.VerificationTokenExpiry.HasValue || account.VerificationTokenExpiry < DateTime.UtcNow)
                {
                    return false; 
                }
                // Đặt tài khoản thành đã xác minh
                account.SetVerified();
                account.ClearVerificationToken();

                await _accountRepository.ReplaceAsync(account.Id.ToString(), account);

                var htmlBody = $@"
                    <h2>Account Verified Successfully!</h2>
                    <p>Hello {account.Fullname ?? account.Username},</p>
                    <p>Your account has been successfully verified. You now have full access to all features of our platform.</p>
                    <p>Thank you for joining us!</p>
                    <p>Best regards,<br/>Stream Cart Team</p>
                ";

                await _emailService.SendEmailAsync(account.Email, "Account Verification Successful", htmlBody, account.Fullname);

                return true;
            }
            catch (Exception ex)
            {               
                return false;
            }
        }
    }
}