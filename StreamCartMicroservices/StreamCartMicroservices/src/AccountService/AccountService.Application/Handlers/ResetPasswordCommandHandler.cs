using AccountService.Application.Commands;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace AccountService.Application.Handlers
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly IAccountRepository _accountRepository;

        public ResetPasswordCommandHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId.ToString());
            
            if (account == null)
            {
                return false;
            }
            try
            {
                // Kiểm tra token và thời gian hết hạn
                if (account.VerificationToken != request.ResetToken)
                {
                    return false; // Token không đúng
                }

                if (!account.VerificationTokenExpiry.HasValue || account.VerificationTokenExpiry < DateTime.UtcNow)
                {
                    return false; // Token đã hết hạn
                }
                string hashedPassword = BC.HashPassword(request.NewPassword);
                account.UpdatePassword(hashedPassword);
                account.ClearVerificationToken();
                await _accountRepository.ReplaceAsync(account.Id.ToString(), account);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}