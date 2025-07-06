using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AccountService.Application.DTOs;
using AccountService.Application.Queries;
using AccountService.Domain.Enums;
using AccountService.Infrastructure.Interfaces;

namespace AccountService.Application.Handlers
{
    public class GetAccountsByShopIdQueryHandler : IRequestHandler<GetAccountsByShopIdQuery, IEnumerable<AccountDto>>
    {
        private readonly IAccountRepository _accountRepository;

        public GetAccountsByShopIdQueryHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<IEnumerable<AccountDto>> Handle(GetAccountsByShopIdQuery request, CancellationToken cancellationToken)
        {
           
            var accounts = await _accountRepository.GetAllAsync();
                
            var shopAccounts = accounts
                .Where(a => a.ShopId == request.ShopId && 
                           (a.Role == RoleType.Seller || a.Role == RoleType.Moderator))
                .ToList();

            var accountDtos = shopAccounts.Select(a => new AccountDto
            {
                Id = a.Id,
                Username = a.Username,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                Fullname = a.Fullname,
                AvatarURL = a.AvatarURL,
                Role = a.Role,
                RegistrationDate = a.RegistrationDate,
                LastLoginDate = a.LastLoginDate,
                IsActive = a.IsActive,
                IsVerified = a.IsVerified,
                CompleteRate = a.CompleteRate,
                ShopId = a.ShopId,
                CreatedAt = a.CreatedAt,
                CreatedBy = a.CreatedBy,
                LastModifiedAt = a.LastModifiedAt,
                LastModifiedBy = a.LastModifiedBy
            }).ToList();

            return accountDtos;
        }
    }
}