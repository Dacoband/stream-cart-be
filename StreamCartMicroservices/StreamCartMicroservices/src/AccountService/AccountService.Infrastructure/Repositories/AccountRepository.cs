using AccountService.Domain.Entities;
using AccountService.Domain.Enums;
using AccountService.Infrastructure.Data;
using AccountService.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountService.Infrastructure.Repositories
{
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        public AccountRepository(AccountContext dbContext) : base(dbContext)
        {
        }

        public async Task<Account?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            return await _dbSet.FirstOrDefaultAsync(a => a.Username == username);
        }

        public async Task<Account?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            return await _dbSet.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            return !await _dbSet.AnyAsync(a => a.Username == username);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            return !await _dbSet.AnyAsync(a => a.Email == email);
        }

        public async Task<IEnumerable<Account>> GetAccountsByRoleAsync(RoleType role)
        {
            return await _dbSet.Where(a => a.Role == role).ToListAsync();
        }
    }
}