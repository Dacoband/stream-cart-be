using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountService.Application.Interfaces
{
    public interface IAccountManagementService
    {
        Task<AccountDto> UpdateAccountAsync(UpdateAccountCommand command);
        Task<bool> DeleteAccountAsync(Guid id);
        Task<AccountDto?> GetAccountByIdAsync(Guid id);
        Task<AccountDto?> GetAccountByUsernameAsync(string username);
        Task<AccountDto?> GetAccountByEmailAsync(string email);
        Task<IEnumerable<AccountDto>> GetAllAccountsAsync();
        Task<PagedResult<AccountDto>> GetAccountsPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<AccountDto>> GetAccountsByRoleAsync(RoleType role);
        Task<bool> IsUsernameUniqueAsync(string username);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<AccountDto> CreateModeratorAccountAsync(CreateAccountCommand command, Guid shopId, Guid createdBySellerAccountId);
        Task<AccountDto> CreateOperationManagerAccountAsync(CreateAccountCommand command, Guid createdByITAdminAccountId);
        Task<AccountDto> UpdateAccountStatusAsync(Guid accountId, bool isActive, Guid updatedByAccountId);
        Task<bool> CanManageAccountStatusAsync(Guid accountId, Guid targetAccountId);
        Task<IEnumerable<AccountDto>> GetAccountsByShopIdAsync(Guid shopId);
        /// <summary>
        /// Kiểm tra xem có thể xóa tài khoản hay không
        /// </summary>
        /// <param name="accountId">ID của tài khoản cần kiểm tra</param>
        /// <returns>Tuple với kết quả kiểm tra và lý do (nếu không thể xóa)</returns>
        Task<(bool CanDelete, string Reason)> CanDeleteAccountAsync(Guid accountId);

        /// <summary>
        /// Đếm số lượng OperationManager đang hoạt động
        /// </summary>
        /// <returns>Số lượng OperationManager active</returns>
        Task<int> CountActiveOperationManagersAsync();
    }
}