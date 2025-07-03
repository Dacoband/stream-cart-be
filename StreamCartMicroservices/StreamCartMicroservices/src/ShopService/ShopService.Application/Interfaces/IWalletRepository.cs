using ShopService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IWalletRepository
    {
        Task<Wallet> GetByIdAsync(string id);
        Task<Wallet> GetByShopIdAsync(Guid shopId);
        Task<Wallet> InsertAsync(Wallet wallet);
        Task<bool> ReplaceAsync(string id, Wallet wallet);
        Task<bool> AddFundsAsync(string walletId, decimal amount, string modifiedBy);
    }
}
