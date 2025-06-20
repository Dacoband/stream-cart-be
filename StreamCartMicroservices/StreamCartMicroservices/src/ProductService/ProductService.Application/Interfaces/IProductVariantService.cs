using ProductService.Application.DTOs.Variants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface IProductVariantService
    {
        Task<IEnumerable<ProductVariantDto>> GetAllAsync();
        Task<ProductVariantDto?> GetByIdAsync(Guid id);
        Task<ProductVariantDto> CreateAsync(CreateProductVariantDto dto, string createdBy);
        Task<ProductVariantDto> UpdateAsync(Guid id, UpdateProductVariantDto dto, string updatedBy);
        Task<bool> DeleteAsync(Guid id, string deletedBy);
        Task<IEnumerable<ProductVariantDto>> GetByProductIdAsync(Guid productId);
        Task<ProductVariantDto> UpdateStockAsync(Guid id, int quantity, string updatedBy);
        Task<ProductVariantDto> UpdatePriceAsync(Guid id, decimal price, decimal? flashSalePrice, string updatedBy);
        Task<bool> BulkUpdateStockAsync(IEnumerable<BulkUpdateStockDto> stockUpdates, string updatedBy);
    }
}