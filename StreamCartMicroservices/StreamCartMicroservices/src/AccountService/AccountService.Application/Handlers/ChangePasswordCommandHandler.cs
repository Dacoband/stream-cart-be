using AccountService.Application.Commands;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace AccountService.Application.Handlers
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly IAccountRepository _accountRepository;

        public ChangePasswordCommandHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId.ToString());
            
            if (account == null)
            {
                return false;
            }

            try
            {
                string hashedPassword = BC.HashPassword(request.NewPassword);
                account.UpdatePassword(hashedPassword);
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