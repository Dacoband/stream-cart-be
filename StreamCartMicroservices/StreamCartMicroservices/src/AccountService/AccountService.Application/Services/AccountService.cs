using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using AccountService.Domain.Entities;
using AccountService.Domain.Enums;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;
using Shared.Common.Domain.Bases;
using Shared.Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountService.Application.Services
{
    public class AccountManagementService : IAccountManagementService
    {
        private readonly IMediator _mediator;
        private readonly IAccountRepository _accountRepository;
        private readonly JwtSettings _jwtSettings;

        public AccountManagementService(
            IMediator mediator, 
            IAccountRepository accountRepository,
            IOptions<JwtSettings> jwtSettings)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        }

        #region Account Management

        public async Task<AccountDto> UpdateAccountAsync(UpdateAccountCommand command)
        {
            return await _mediator.Send(command);
        }

        public async Task<bool> DeleteAccountAsync(Guid id)
        {
            try
            {
                await _accountRepository.DeleteAsync(id.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<AccountDto?> GetAccountByIdAsync(Guid id)
        {
            var account = await _accountRepository.GetByIdAsync(id.ToString());
            return MapToDto(account);
        }

        public async Task<AccountDto?> GetAccountByUsernameAsync(string username)
        {
            var account = await _accountRepository.GetByUsernameAsync(username);
            return MapToDto(account);
        }

        public async Task<AccountDto?> GetAccountByEmailAsync(string email)
        {
            var account = await _accountRepository.GetByEmailAsync(email);
            return MapToDto(account);
        }

        public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
        {
            var accounts = await _accountRepository.GetAllAsync();
            return accounts.Select(MapToDto).Where(dto => dto != null).ToList();
        }

        public async Task<PagedResult<AccountDto>> GetAccountsPagedAsync(int pageNumber, int pageSize)
        {
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var pagedAccounts = await _accountRepository.SearchAsync(string.Empty, paginationParams);

            var accountDtos = pagedAccounts.Items.Select(MapToDto).Where(dto => dto != null).ToList();

            return new PagedResult<AccountDto>(
                accountDtos,
                pagedAccounts.TotalCount,
                pagedAccounts.CurrentPage,
                pagedAccounts.PageSize
            );
        }

        public async Task<IEnumerable<AccountDto>> GetAccountsByRoleAsync(RoleType role)
        {
            var accounts = await _accountRepository.GetAccountsByRoleAsync(role);
            return accounts.Select(MapToDto).Where(dto => dto != null).ToList();
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            return await _accountRepository.IsUsernameUniqueAsync(username);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return await _accountRepository.IsEmailUniqueAsync(email);
        }
        public async Task<AccountDto> CreateModeratorAccountAsync(CreateAccountCommand command, Guid shopId, Guid createdBySellerAccountId)
        {
            var creatorAccount = await _accountRepository.GetByIdAsync(createdBySellerAccountId.ToString());
            if (creatorAccount == null || creatorAccount.Role != RoleType.Seller)
            {
                throw new UnauthorizedAccessException("Only Sellers can create Moderator accounts");
            }

            if (creatorAccount.ShopId != shopId)
            {
                throw new InvalidOperationException("The Moderator must be assigned to the Seller's shop");
            }

            command.Role = RoleType.Moderator;
            command.IsActive = true;
            command.IsVerified = false; 

            var accountDto = await _mediator.Send(command);

            var updateCommand = new UpdateAccountCommand
            {
                Id = accountDto.Id,
                ShopId = shopId,
                UpdatedBy = createdBySellerAccountId.ToString()
            };

            return await UpdateAccountAsync(updateCommand);
        }

        public async Task<AccountDto> CreateOperationManagerAccountAsync(CreateAccountCommand command, Guid createdByITAdminAccountId)
        {
            var creatorAccount = await _accountRepository.GetByIdAsync(createdByITAdminAccountId.ToString());
            if (creatorAccount == null || creatorAccount.Role != RoleType.ITAdmin)
            {
                throw new UnauthorizedAccessException("Only IT Admins can create Operation Manager accounts");
            }

            command.Role = RoleType.OperationManager;
            command.IsActive = true;
            command.IsVerified = false; 

            return await _mediator.Send(command);
        }
        public async Task<AccountDto> UpdateAccountStatusAsync(Guid accountId, bool isActive, Guid updatedByAccountId)
        {
            if (!await CanManageAccountStatusAsync(updatedByAccountId, accountId))
            {
                throw new UnauthorizedAccessException("You don't have permission to change this account's status");
            }

            var updateCommand = new UpdateAccountCommand
            {
                Id = accountId,
                IsActive = isActive,
                UpdatedBy = updatedByAccountId.ToString()
            };
            return await _mediator.Send(updateCommand);
        }

        public async Task<bool> CanManageAccountStatusAsync(Guid managerId, Guid targetAccountId)
        {
            var manager = await _accountRepository.GetByIdAsync(managerId.ToString());
            if (manager == null)
                return false;

            var targetAccount = await _accountRepository.GetByIdAsync(targetAccountId.ToString());
            if (targetAccount == null)
                return false;

            if (manager.Role == RoleType.ITAdmin)
                return true;

            if (manager.Role == RoleType.Seller && 
                targetAccount.Role == RoleType.Moderator && 
                manager.ShopId.HasValue && 
                targetAccount.ShopId.HasValue && 
                manager.ShopId == targetAccount.ShopId)
                return true;

            return false;
        }
        #endregion

        private AccountDto? MapToDto(Account? account)
        {
            if (account == null)
                return null;

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
