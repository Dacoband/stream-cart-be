using ProductService.Application.DTOs.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface IProductAttributeService
    {
        Task<IEnumerable<ProductAttributeDto>> GetAllAsync();
        Task<ProductAttributeDto?> GetByIdAsync(Guid id);
        Task<ProductAttributeDto> CreateAsync(CreateProductAttributeDto dto, string createdBy);
        Task<ProductAttributeDto> UpdateAsync(Guid id, UpdateProductAttributeDto dto, string updatedBy);
        Task<bool> DeleteAsync(Guid id, string deletedBy);
        Task<IEnumerable<ProductAttributeDto>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<AttributeValueDto>> GetValuesByAttributeIdAsync(Guid attributeId);
    }
}