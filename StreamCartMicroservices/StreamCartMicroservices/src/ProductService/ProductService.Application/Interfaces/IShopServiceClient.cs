using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Products;
using System;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface IShopServiceClient
    {
        /// <summary>
        /// Kiểm tra shop có tồn tại không
        /// </summary>
        /// <param name="shopId">ID của shop cần kiểm tra</param>
        /// <returns>true nếu shop tồn tại, ngược lại false</returns>
        Task<bool> DoesShopExistAsync(Guid shopId);

        /// <summary>
        /// Lấy thông tin cơ bản của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <returns>Thông tin shop hoặc null nếu không tìm thấy</returns>
        Task<ShopDto?> GetShopByIdAsync(Guid shopId);
        /// <summary>
        /// Lấy thông tin chi tiết của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <returns>Thông tin chi tiết shop hoặc null nếu không tìm thấy</returns>
        Task<ShopDetailDto> GetShopByIdAsyncDetail(Guid shopId);

        /// <summary>
        /// Cập nhật số lượng sản phẩm cho shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="productCount">Số lượng sản phẩm mới</param>
        /// <returns>true nếu cập nhật thành công</returns>
        Task<bool> UpdateShopProductCountAsync(Guid shopId, int productCount);
    }
}