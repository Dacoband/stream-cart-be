using AccountService.Domain.Entities;
using AccountService.Domain.Enums;
using Shared.Common.Data.Interfaces;

namespace AccountService.Infrastructure.Interfaces
{
    public interface IAccountRepository : IGenericRepository<Account>
    {
        Task<Account?> GetByUsernameAsync(string username);
        Task<Account?> GetByEmailAsync(string email);
        Task<bool> IsUsernameUniqueAsync(string username);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<IEnumerable<Account>> GetAccountsByRoleAsync(RoleType role);
    }
}