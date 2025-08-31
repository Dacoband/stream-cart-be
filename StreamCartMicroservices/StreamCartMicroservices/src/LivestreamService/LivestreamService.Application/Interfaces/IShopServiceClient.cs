using Livestreamservice.Application.DTOs;
using LivestreamService.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface IShopServiceClient
    {
        /// <summary>
        /// Gets shop details by ID
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>Shop details</returns>
        Task<ShopDTO> GetShopByIdAsync(Guid shopId);

        /// <summary>
        /// Checks if an account is a member of a shop
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="accountId">Account ID</param>
        /// <returns>True if the account is a member of the shop, otherwise false</returns>
        Task<bool> IsShopMemberAsync(Guid shopId, Guid accountId);

        /// <summary>
        /// Gets shop address information
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>Shop address information</returns>
        Task<AddressOfShop> GetShopAddressAsync(Guid shopId);

        /// <summary>
        /// Updates shop completion rate
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="changeAmount">Change amount</param>
        /// <param name="updatedByAccountId">ID of the account making the update</param>
        /// <returns>True if successful, otherwise false</returns>
        Task<bool> UpdateShopCompletionRateAsync(Guid shopId, decimal changeAmount, Guid updatedByAccountId);

        /// <summary>
        /// Checks if a shop exists
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>True if the shop exists, otherwise false</returns>
        Task<bool> DoesShopExistAsync(Guid shopId);
        Task<ShopMembershipDto?> GetActiveShopMembershipAsync(Guid shopId);
        Task<bool> UpdateShopMembershipRemainingLivestreamAsync(Guid shopId, int remainingMinutes);


    }
}