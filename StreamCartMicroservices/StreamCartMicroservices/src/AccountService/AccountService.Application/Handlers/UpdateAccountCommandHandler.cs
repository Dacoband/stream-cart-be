using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers
{
    public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, AccountDto>
    {
        private readonly IAccountRepository _accountRepository;

        public UpdateAccountCommandHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<AccountDto> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            // Lấy tài khoản từ ID
            var account = await _accountRepository.GetByIdAsync(request.Id.ToString());
            
            if (account == null)
            {
                throw new ApplicationException($"Account with ID {request.Id} not found");
            }

            // Cập nhật thông tin
            if (request.PhoneNumber != null || request.Fullname != null || request.AvatarURL != null)
            {
                account.UpdateProfile(request.Fullname, request.PhoneNumber, request.AvatarURL);
            }

            // Cập nhật role nếu có
            if (request.Role.HasValue)
            {
                account.UpdateRole(request.Role.Value);
            }

            // Cập nhật trạng thái active nếu có
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                {
                    account.Activate();
                }
                else
                {
                    account.Deactivate();
                }
            }

            // Cập nhật trạng thái verified nếu có
            if (request.IsVerified.HasValue && request.IsVerified.Value)
            {
                account.SetVerified();
            }

            // Cập nhật CompleteRate nếu có
            if (request.CompleteRate.HasValue)
            {
                account.UpdateCompleteRate(request.CompleteRate.Value);
            }

            // Cập nhật ShopId nếu có
            if (request.ShopId.HasValue)
            {
                account.UpdateShopId(request.ShopId.Value);
            }

            // Cập nhật thông tin người cập nhật nếu có
            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                account.SetUpdatedBy(request.UpdatedBy);
            }

            // Lưu thay đổi vào database
            await _accountRepository.ReplaceAsync(account.Id.ToString(), account);

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
