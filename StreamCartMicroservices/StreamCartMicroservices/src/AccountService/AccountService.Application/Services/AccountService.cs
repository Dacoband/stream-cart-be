using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using AccountService.Application.Queries;
using AccountService.Domain.Entities;
using AccountService.Domain.Enums;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AccountManagementService> _logger;
        private readonly JwtSettings _jwtSettings;

        public AccountManagementService(
            IMediator mediator,
            IAccountRepository accountRepository,
            ILogger<AccountManagementService> logger,
            IOptions<JwtSettings> jwtSettings)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        public async Task<IEnumerable<AccountDto>> GetAccountsByShopIdAsync(Guid shopId)
        {
            var query = new GetAccountsByShopIdQuery { ShopId = shopId };
            var result = await _mediator.Send(query);
            return result;
        }

        public async Task<(bool CanDelete, string Reason)> CanDeleteAccountAsync(Guid accountId)
        {
            var account = await GetAccountByIdAsync(accountId);
            if (account == null)
            {
                return (false, "Account not found");
            }

            // Không được xóa ITAdmin
            if (account.Role == RoleType.ITAdmin)
            {
                return (false, "Cannot delete ITAdmin account");
            }

            // Kiểm tra OperationManager
            if (account.Role == RoleType.OperationManager)
            {
                var activeOpManagerCount = await CountActiveOperationManagersAsync();
                // Trừ đi chính tài khoản này nếu nó đang active
                if (account.IsActive)
                {
                    activeOpManagerCount--;
                }

                if (activeOpManagerCount < 1)
                {
                    return (false, "Cannot delete the last active Operation Manager. At least one Operation Manager must exist in the system.");
                }
            }

            return (true, string.Empty);
        }

        public async Task<int> CountActiveOperationManagersAsync()
        {
            var operationManagers = await GetAccountsByRoleAsync(RoleType.OperationManager);
            return operationManagers.Count(om => om.IsActive);
        }

        public async Task<bool> RemoveModeratorFromShopAsync(Guid moderatorId, Guid shopId, Guid removedByAccountId)
        {
            try
            {
                // Lấy thông tin moderator
                var moderator = await _accountRepository.GetByIdAsync(moderatorId.ToString());
                if (moderator == null)
                {
                    _logger.LogWarning("Moderator {ModeratorId} không tồn tại", moderatorId);
                    return false;
                }

                // Kiểm tra có phải moderator không
                if (moderator.Role != RoleType.Moderator)
                {
                    _logger.LogWarning("Account {AccountId} không phải là moderator", moderatorId);
                    return false;
                }

                // Kiểm tra moderator có thuộc shop này không
                if (moderator.ShopId != shopId)
                {
                    _logger.LogWarning("Moderator {ModeratorId} không thuộc shop {ShopId}", moderatorId, shopId);
                    return false;
                }

                // Vô hiệu hóa tài khoản - sử dụng method có sẵn
                moderator.Deactivate();
                moderator.SetUpdatedBy(removedByAccountId.ToString());

                // Cập nhật vào database
                await _accountRepository.ReplaceAsync(moderator.Id.ToString(), moderator);

                // Gỡ khỏi shop
                var updateCommand = new UpdateAccountCommand
                {
                    Id = moderatorId,
                    ShopId = null, // Remove from shop
                    UpdatedBy = removedByAccountId.ToString()
                };

                await UpdateAccountAsync(updateCommand);

                _logger.LogInformation("Đã xóa moderator {ModeratorId} khỏi shop {ShopId} bởi {RemovedBy}",
                    moderatorId, shopId, removedByAccountId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa moderator {ModeratorId} khỏi shop {ShopId}", moderatorId, shopId);
                return false;
            }
        }

        public async Task<IEnumerable<AccountDto>> GetInactiveModeratorsByShopAsync(Guid shopId)
        {
            try
            {
                var accounts = await _accountRepository.GetAllAsync();
                var inactiveModerators = accounts
                    .Where(a => a.Role == RoleType.Moderator &&
                               a.ShopId == shopId &&
                               !a.IsActive)
                    .Select(a => new AccountDto
                    {
                        Id = a.Id,
                        Username = a.Username,
                        Email = a.Email,
                        PhoneNumber = a.PhoneNumber,
                        Fullname = a.Fullname,
                        Role = a.Role, // Use Domain enum directly
                        IsActive = a.IsActive,
                        IsVerified = a.IsVerified,
                        ShopId = a.ShopId,
                        RegistrationDate = a.RegistrationDate,
                        LastLoginDate = a.LastLoginDate,
                        AvatarURL = a.AvatarURL,
                        CompleteRate = a.CompleteRate,
                        CreatedAt = a.CreatedAt,
                        CreatedBy = a.CreatedBy,
                        LastModifiedAt = a.LastModifiedAt,
                        LastModifiedBy = a.LastModifiedBy
                    });

                return inactiveModerators;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách moderator không hoạt động của shop {ShopId}", shopId);
                return Enumerable.Empty<AccountDto>();
            }
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