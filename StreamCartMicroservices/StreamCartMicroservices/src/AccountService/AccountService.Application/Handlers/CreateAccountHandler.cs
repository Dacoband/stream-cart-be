using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Domain.Entities;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;
using BC = BCrypt.Net.BCrypt;

namespace AccountService.Application.Handlers
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
    {
        private readonly IAccountRepository _accountRepository;

        public CreateAccountCommandHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            string hashedPassword;
            try
            {
                hashedPassword = BC.HashPassword(request.Password);
            }
            catch
            {
                hashedPassword = request.Password;
            }

            // Tạo account mới
            var account = new Account(
                request.Username,
                hashedPassword,
                request.Email,
                request.Role
            );

            // Cập nhật thêm thông tin
            account.UpdateProfile(request.Fullname, request.PhoneNumber, request.AvatarURL);

            if (!request.IsActive)
            {
                account.Deactivate();
            }

            if (request.IsVerified)
            {
                account.SetVerified();
            }
            await _accountRepository.InsertAsync(account);

            var accountDto = new AccountDto
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

            return accountDto;
        }
    }
}