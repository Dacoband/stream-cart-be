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
    }
}