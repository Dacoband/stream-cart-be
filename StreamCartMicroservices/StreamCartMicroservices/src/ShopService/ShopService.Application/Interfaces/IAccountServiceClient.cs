using System;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IAccountServiceClient
    {
        Task UpdateAccountShopInfoAsync(Guid accountId, Guid shopId);
        Task<string> GetEmailByAccountIdAsync(Guid accountId);
    }
}