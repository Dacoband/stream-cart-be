using ShopService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    /// <summary>
    /// Client để gọi Product Service
    /// </summary>
    public interface IProductServiceClient
    {
        /// <summary>
        /// Lấy danh sách sản phẩm của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="activeOnly">Chỉ lấy sản phẩm đang hoạt động</param>
        /// <returns>Danh sách sản phẩm</returns>
        Task<List<ProductDto>> GetProductsByShopIdAsync(Guid shopId, bool activeOnly = true);

        /// <summary>
        /// Đếm số lượng sản phẩm của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="activeOnly">Chỉ đếm sản phẩm đang hoạt động</param>
        /// <returns>Số lượng sản phẩm</returns>
        Task<int> GetProductCountByShopIdAsync(Guid shopId, bool activeOnly = true);

        /// <summary>
        /// Lấy thông tin sản phẩm theo ID
        /// </summary>
        /// <param name="productId">ID của sản phẩm</param>
        /// <returns>Thông tin sản phẩm hoặc null nếu không tìm thấy</returns>
        Task<ProductDto?> GetProductByIdAsync(Guid productId);

        /// <summary>
        /// Kiểm tra sản phẩm có tồn tại không
        /// </summary>
        /// <param name="productId">ID của sản phẩm</param>
        /// <returns>true nếu sản phẩm tồn tại</returns>
        Task<bool> DoesProductExistAsync(Guid productId);

        /// <summary>
        /// Kiểm tra sản phẩm có thuộc về shop không
        /// </summary>
        /// <param name="productId">ID của sản phẩm</param>
        /// <param name="shopId">ID của shop</param>
        /// <returns>true nếu sản phẩm thuộc về shop</returns>
        Task<bool> IsProductOwnedByShopAsync(Guid productId, Guid shopId);

        /// <summary>
        /// Cập nhật số lượng tồn kho của sản phẩm
        /// </summary>
        /// <param name="productId">ID của sản phẩm</param>
        /// <param name="quantityChange">Số lượng thay đổi (âm để giảm, dương để tăng)</param>
        /// <returns>true nếu cập nhật thành công</returns>
        Task<bool> UpdateProductStockAsync(Guid productId, int quantityChange);

        /// <summary>
        /// Cập nhật trạng thái hoạt động của sản phẩm
        /// </summary>
        /// <param name="productId">ID của sản phẩm</param>
        /// <param name="isActive">Trạng thái hoạt động</param>
        /// <returns>true nếu cập nhật thành công</returns>
        Task<bool> UpdateProductStatusAsync(Guid productId, bool isActive);
        Task<TopProductsDTO> GetTopAIRecommendedProductsAsync(Guid shopId, DateTime fromDate, DateTime toDate, int limit = 5);

    }
}