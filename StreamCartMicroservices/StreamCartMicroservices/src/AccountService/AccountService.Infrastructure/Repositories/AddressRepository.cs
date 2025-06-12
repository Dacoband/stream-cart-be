using AccountService.Domain.Entities;
using AccountService.Domain.Enums;
using AccountService.Infrastructure.Data;
using AccountService.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Infrastructure.Repositories
{
    public class AddressRepository : EfCoreGenericRepository<Address>, IAddressRepository
    {
        private readonly AccountContext _accountContext;

        public AddressRepository(AccountContext accountContext) : base(accountContext)
        {
            _accountContext = accountContext;
        }

        public async Task<IEnumerable<Address>> GetByAccountIdAsync(Guid accountId)
        {
            return await _dbSet
                .Where(a => a.AccountId == accountId && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Address>> GetByShopIdAsync(Guid? shopId)
        {
            if (!shopId.HasValue)
                return new List<Address>();

            return await _dbSet
                .Where(a => a.ShopId == shopId && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<Address?> GetDefaultShippingAddressByAccountIdAsync(Guid accountId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.IsDefaultShipping && !a.IsDeleted);
        }

        public async Task<IEnumerable<Address>> GetAddressesByTypeAsync(Guid accountId, AddressType type)
        {
            return await _dbSet
                .Where(a => a.AccountId == accountId && a.Type == type && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> SetDefaultShippingAddressAsync(string addressId, Guid accountId)
        {
            if (!Guid.TryParse(addressId, out var id))
                return false;

            using var transaction = await _accountContext.Database.BeginTransactionAsync();
            try
            {
                // Đầu tiên, bỏ đánh dấu tất cả các địa chỉ mặc định hiện tại
                await UnsetAllDefaultShippingAddressesAsync(accountId);

                // Tiếp theo, đánh dấu địa chỉ được chọn là mặc định
                var address = await _dbSet.FirstOrDefaultAsync(a => a.Id == id && a.AccountId == accountId && !a.IsDeleted);
                if (address == null)
                    return false;

                address.SetAsDefaultShipping();
                _accountContext.Entry(address).State = EntityState.Modified;
                await _accountContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> UnsetAllDefaultShippingAddressesAsync(Guid accountId)
        {
            try
            {
                var defaultAddresses = await _dbSet
                    .Where(a => a.AccountId == accountId && a.IsDefaultShipping && !a.IsDeleted)
                    .ToListAsync();

                foreach (var address in defaultAddresses)
                {
                    address.UnsetAsDefaultShipping();
                    _accountContext.Entry(address).State = EntityState.Modified;
                }

                await _accountContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
