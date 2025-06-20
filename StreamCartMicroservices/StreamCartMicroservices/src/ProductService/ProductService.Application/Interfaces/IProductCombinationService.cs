using ProductService.Application.DTOs.Combinations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface IProductCombinationService
    {
        Task<IEnumerable<ProductCombinationDto>> GetAllAsync();
        Task<IEnumerable<ProductCombinationDto>> GetByVariantIdAsync(Guid variantId);
        Task<ProductCombinationDto> CreateAsync(CreateProductCombinationDto dto, string createdBy);
        Task<ProductCombinationDto> UpdateAsync(Guid variantId, Guid attributeValueId, UpdateProductCombinationDto dto, string updatedBy);
        Task<bool> DeleteAsync(Guid variantId, Guid attributeValueId, string deletedBy);
        Task<IEnumerable<ProductCombinationDto>> GetByProductIdAsync(Guid productId);
        Task<bool> GenerateCombinationsAsync(Guid productId, List<AttributeValueGroup> attributeValueGroups, decimal defaultPrice, int defaultStock, string createdBy);
    }
}