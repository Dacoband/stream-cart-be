using AccountService.Application.Commands;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace AccountService.Application.Handlers
{
    public class VerifyPasswordCommandHandler : IRequestHandler<VerifyPasswordCommand, bool>
    {
        private readonly IAccountRepository _accountRepository;

        public VerifyPasswordCommandHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<bool> Handle(VerifyPasswordCommand request, CancellationToken cancellationToken)
        {
            // Lấy tài khoản từ ID
            var account = await _accountRepository.GetByIdAsync(request.AccountId.ToString());

            if (account == null)
            {
                return false;
            }

            // Kiểm tra mật khẩu bằng BCrypt
            return BC.Verify(request.Password, account.Password);
        }
    }
}