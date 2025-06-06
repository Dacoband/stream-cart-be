using AccountService.Application.Commands;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers
{
    public class UpdateLastLoginCommandHandler : IRequestHandler<UpdateLastLoginCommand, bool>
    {
        private readonly IAccountRepository _accountRepository;

        public UpdateLastLoginCommandHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<bool> Handle(UpdateLastLoginCommand request, CancellationToken cancellationToken)
        {
            // L?y tài kho?n t? ID
            var account = await _accountRepository.GetByIdAsync(request.AccountId.ToString());
            
            if (account == null)
            {
                return false;
            }

            // C?p nh?t th?i gian ??ng nh?p cu?i
            account.RecordLogin();

            // C?p nh?t l?i vào c? s? d? li?u
            await _accountRepository.ReplaceAsync(account.Id.ToString(), account);

            return true;
        }
    }
}