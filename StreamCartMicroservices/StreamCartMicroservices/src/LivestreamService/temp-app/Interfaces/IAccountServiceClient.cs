using Livestreamservice.Application.DTOs;
using LivestreamService.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface IAccountServiceClient
    {
        /// <summary>
        /// Gets account information by ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Account information</returns>
        Task<AccountDTO> GetAccountByIdAsync(Guid accountId);

        /// <summary>
        /// Gets seller information by ID
        /// </summary>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>Seller information</returns>
        Task<SellerDTO> GetSellerByIdAsync(Guid sellerId);
    }
}