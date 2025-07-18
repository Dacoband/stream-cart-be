using LivestreamService.Application.DTOs;
using LivestreamService.Application.DTOs.LiveStreamProduct;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface IProductServiceClient
    {
        /// <summary>
        /// Lấy thông tin sản phẩm theo ID
        /// </summary>
        Task<ProductDTO?> GetProductByIdAsync(string productId);

        /// <summary>
        /// Lấy thông tin variant của sản phẩm
        /// </summary>
        Task<ProductVariantDTO> GetProductVariantAsync(string productId, string variantId);

        /// <summary>
        /// Kiểm tra xem sản phẩm có thuộc về shop không
        /// </summary>
        Task<bool> IsProductOwnedByShopAsync(string productId, Guid shopId);

        /// <summary>
        /// Lấy thông tin FlashSale theo ID
        /// </summary>
        Task<FlashSaleDTO> GetFlashSaleByIdAsync(Guid flashSaleId);

        /// <summary>
        /// Kiểm tra FlashSale có hợp lệ cho sản phẩm/variant không
        /// </summary>
        Task<bool> IsFlashSaleValidForProductAsync(Guid flashSaleId, string productId, string? variantId = null);

        // Helper methods
        Task<ProductDTO?> GetProductByIdAsync(Guid productId);
        Task<List<ProductDTO>> GetProductsByShopIdAsync(Guid shopId);
        Task<List<ProductDTO>> GetProductsWithFlashSaleAsync();
        Task<bool> UpdateProductStatusAsync(Guid productId, bool isActive);
        Task<bool> CheckProductExistsAsync(Guid productId);
    }
}