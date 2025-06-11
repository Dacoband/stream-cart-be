using AccountService.Application.Commands;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers
{
    public class VerifyOTPCommandHandler : IRequestHandler<VerifyOTPCommand, bool>
    {
        private readonly IAccountRepository _accountRepository;

        public VerifyOTPCommandHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<bool> Handle(VerifyOTPCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId.ToString());
            
            if (account == null)
            {
                return false;
            }

            if (account.VerificationToken != request.OTP)
            {
                return false; 
            }

            if (!account.VerificationTokenExpiry.HasValue || account.VerificationTokenExpiry < DateTime.UtcNow)
            {
                return false; 
            }
            account.SetVerified();
            account.ClearVerificationToken();

            await _accountRepository.ReplaceAsync(account.Id.ToString(), account);

            return true;
        }
    }
}