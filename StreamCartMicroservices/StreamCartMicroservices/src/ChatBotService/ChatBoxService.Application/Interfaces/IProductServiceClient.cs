using ChatBoxService.Application.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBoxService.Application.Interfaces
{
    /// <summary>
    /// Client interface for calling Product Service from ChatBot Service
    /// </summary>
    public interface IProductServiceClient
    {
        /// <summary>
        /// Lấy thông tin sản phẩm theo ID
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Product details if found, null otherwise</returns>
        Task<ProductDto?> GetProductByIdAsync(Guid productId);

        /// <summary>
        /// Lấy danh sách sản phẩm của shop
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="activeOnly">Chỉ lấy sản phẩm đang hoạt động</param>
        /// <returns>List of products</returns>
        Task<List<ProductDto>> GetProductsByShopIdAsync(Guid shopId, bool activeOnly = true);

        /// <summary>
        /// Kiểm tra sản phẩm có thuộc về shop không
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="shopId">Shop ID</param>
        /// <returns>true if product belongs to shop</returns>
        Task<bool> IsProductOwnedByShopAsync(Guid productId, Guid shopId);

        /// <summary>
        /// Kiểm tra sản phẩm có tồn tại không
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>true if product exists</returns>
        Task<bool> DoesProductExistAsync(Guid productId);

        /// <summary>
        /// Đếm số lượng sản phẩm của shop
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="activeOnly">Chỉ đếm sản phẩm đang hoạt động</param>
        /// <returns>Số lượng sản phẩm</returns>
        Task<int> GetProductCountByShopIdAsync(Guid shopId, bool activeOnly = true);
    }
}