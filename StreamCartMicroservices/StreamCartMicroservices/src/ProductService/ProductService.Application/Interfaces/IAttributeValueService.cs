using ProductService.Application.DTOs.Attributes;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface IAttributeValueService
    {
        Task<IEnumerable<AttributeValueDto>> GetAllAsync();
        Task<AttributeValueDto?> GetByIdAsync(Guid id);
        Task<AttributeValueDto> CreateAsync(CreateAttributeValueDto dto, string createdBy);
        Task<AttributeValueDto> UpdateAsync(Guid id, UpdateAttributeValueDto dto, string updatedBy);
        Task<bool> DeleteAsync(Guid id, string deletedBy);
        Task<IEnumerable<AttributeValueDto>> GetByAttributeIdAsync(Guid attributeId);
    }
}