using ShopService.Application.DTOs;
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
        /// <summary>
        /// Lấy thông tin account theo Shop ID
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <returns>Danh sách account của shop</returns>
        Task<List<ShopAccountDTO>> GetAccountsByShopIdAsync(Guid shopId);
    }
}