using ChatBoxService.Application.DTOs.ShopDto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBoxService.Application.Interfaces
{
    /// <summary>
    /// Client interface for calling Shop Service from ChatBot Service
    /// </summary>
    public interface IShopServiceClient
    {
        /// <summary>
        /// Lấy thông tin shop theo ID
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>Shop details if found, null otherwise</returns>
        Task<ShopDto?> GetShopByIdAsync(Guid shopId);

        /// <summary>
        /// Kiểm tra shop có tồn tại không
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>true if shop exists</returns>
        Task<bool> DoesShopExistAsync(Guid shopId);

        /// <summary>
        /// Kiểm tra shop có đang hoạt động không
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>true if shop is active</returns>
        Task<bool> IsShopActiveAsync(Guid shopId);

        /// <summary>
        /// Lấy danh sách shop theo trạng thái
        /// </summary>
        /// <param name="isActive">Trạng thái hoạt động</param>
        /// <returns>List of shops</returns>
        Task<List<ShopDto>> GetShopsByStatusAsync(bool isActive);

        /// <summary>
        /// Tìm kiếm shop theo tên
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <returns>List of matching shops</returns>
        Task<List<ShopDto>> SearchShopsByNameAsync(string searchTerm);
    }
}