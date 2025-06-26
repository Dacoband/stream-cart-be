using AccountService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountService.Application.Interfaces
{
    public interface IShopServiceClient
    {
        /// <summary>
        /// Lấy thông tin shop theo ID
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        Task<ShopDto> GetShopByIdAsync(Guid shopId);
        
        /// <summary>
        /// Lấy danh sách shop của một account
        /// </summary>
        /// <param name="accountId">ID của tài khoản</param>
        Task<IEnumerable<ShopDto>> GetShopsByAccountIdAsync(Guid accountId);
        
        /// <summary>
        /// Kiểm tra quyền của account đối với shop
        /// </summary>
        /// <param name="accountId">ID của tài khoản</param>
        /// <param name="shopId">ID của shop</param>
        /// <param name="role">Vai trò cần kiểm tra, ví dụ: "Owner"</param>
        Task<bool> HasShopPermissionAsync(Guid accountId, Guid shopId, string role);
    }
}