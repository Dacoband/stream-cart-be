using AccountService.Domain.Entities;
using AccountService.Domain.Enums;
using Shared.Common.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountService.Infrastructure.Interfaces
{
    public interface IAddressRepository : IGenericRepository<Address>
    {
        /// <summary>
        /// Dùng để lấy địa chỉ theo ID của tài khoản (AccountId).
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        Task<IEnumerable<Address>> GetByAccountIdAsync(Guid accountId);
        /// <summary>
        /// Dùng để lấy danh sách địa chỉ theo ID của cửa hàng (ShopId).
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        Task<IEnumerable<Address>> GetByShopIdAsync(Guid? shopId);
      /// <summary>
        /// Dùng để lấy địa chỉ mặc định giao hàng theo ID của tài khoản (AccountId).
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        Task<Address?> GetDefaultShippingAddressByAccountIdAsync(Guid accountId);
        /// <summary>
        /// Dùng để lấy danh sách địa chỉ theo loại địa chỉ (AddressType) và ID của tài khoản (AccountId).
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<IEnumerable<Address>> GetAddressesByTypeAsync(Guid accountId, AddressType type);
        /// <summary>
        /// Dùng để lấy địa chỉ theo ID của cửa hàng (ShopId) và loại địa chỉ (AddressType).
        /// </summary>
        /// <param name="addressId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        Task<bool> SetDefaultShippingAddressAsync(string addressId, Guid accountId);
        /// <summary>
        /// Dùng để hủy đặt tất cả địa chỉ giao hàng mặc định cho tài khoản (AccountId).
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        Task<bool> UnsetAllDefaultShippingAddressesAsync(Guid accountId);
    }
}
