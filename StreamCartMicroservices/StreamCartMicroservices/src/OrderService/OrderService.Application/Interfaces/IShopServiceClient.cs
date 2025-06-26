using System;
using System.Threading.Tasks;
using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces
{
    public interface IShopServiceClient
    {
        /// <summary>
        /// Gets shop details by ID
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>Shop details</returns>
        Task<ShopDto> GetShopByIdAsync(Guid shopId);

        /// <summary>
        /// Checks if a shop is active
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>True if the shop is active, otherwise false</returns>
        Task<bool> IsShopActiveAsync(Guid shopId);
        Task<bool> IsShopMemberAsync(Guid shopId, Guid accountId);
        /// <summary>
        /// Gets shop address information for order creation
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>Shop address information</returns>
        Task<AddressOfShop> GetShopAddressAsync(Guid shopId);

    }
}