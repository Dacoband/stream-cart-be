using System;
using System.Collections.Generic;
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
        Task<SellerDTO?> GetSellerByIdAsync(Guid sellerId);

        /// <summary>
        /// Gets email address by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Email address</returns>
        Task<string> GetEmailByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Checks if user exists
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if user exists</returns>
        Task<bool> DoesUserExistAsync(Guid userId);

        /// <summary>
        /// Gets accounts by shop ID
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>List of accounts</returns>
        Task<List<AccountDTO>> GetAccountByShopIdAsync(Guid shopId);
    }

    public class AccountDTO
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Fullname { get; set; }
        public int? Role { get; set; }
        public bool IsVerified { get; set; }
        public Guid? ShopId { get; set; } // ✅ Thêm ShopId property
    }

    public class SellerDTO
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Fullname { get; set; }
        public string? AvatarUrl { get; set; }
        public Guid ShopId { get; set; }
    }
}