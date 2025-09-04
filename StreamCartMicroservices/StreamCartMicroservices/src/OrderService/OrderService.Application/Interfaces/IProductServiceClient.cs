using System;
using System.Threading.Tasks;
using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces.IServices
{
    public interface IProductServiceClient
    {
        /// <summary>
        /// Gets product details by ID from the Product service
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Product details if found, null otherwise</returns>
        Task<ProductDto> GetProductByIdAsync(Guid productId);
        
        /// <summary>
        /// Gets variant details by ID from the Product service
        /// </summary>
        /// <param name="variantId">Variant ID</param>
        /// <returns>Variant details if found, null otherwise</returns>
        Task<VariantDto> GetVariantByIdAsync(Guid variantId);
        
        /// <summary>
        /// Updates product stock quantity in the Product service
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="quantity">Quantity change (negative for decrease)</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdateProductStockAsync(Guid productId, int quantity);
        
        /// <summary>
        /// Updates variant stock quantity in the Product service
        /// </summary>
        /// <param name="variantId">Variant ID</param>
        /// <param name="quantity">Quantity change (negative for decrease)</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdateVariantStockAsync(Guid variantId, int quantity);
        Task<bool> UpdateProductQuantitySoldAsync(Guid productId, int quantityChange);
        Task<List<FlashSaleDetailDTO>> GetCurrentFlashSalesAsync();
        Task<bool> IncreaseFlashSaleSoldAsync(Guid flashSaleId, int quantity);
    

    }
}
