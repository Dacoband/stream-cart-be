using ShopService.Application.DTOs.Account;
using ShopService.Application.Services;
using System;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IAccountServiceClient
    {
        Task UpdateAccountShopInfoAsync(Guid accountId, Guid shopId);
        Task<string> GetEmailByAccountIdAsync(Guid accountId);
        Task<AccountDetailDTO> GetAccountByAccountIdAsync(Guid accountId);
    }
}