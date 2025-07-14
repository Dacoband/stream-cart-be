using OrderService.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces
{
    /// <summary>
    /// Client for the Account Service
    /// </summary>
    public interface IAccountServiceClient
    {
        /// <summary>
        /// Gets account details by ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Account details</returns>
        Task<AccountDto> GetAccountByIdAsync(Guid accountId);
        
        /// <summary>
        /// Gets email address by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Email address</returns>
        Task<string> GetEmailByAccountIdAsync(Guid accountId);
        Task<List<AccountDto?>> GetAccountByShopIdAsync(Guid shopId);
    }
}