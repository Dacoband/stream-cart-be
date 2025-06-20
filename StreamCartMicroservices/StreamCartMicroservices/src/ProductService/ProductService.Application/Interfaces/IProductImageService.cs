using Microsoft.AspNetCore.Http;
using ProductService.Application.DTOs.Images;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface IProductImageService
    {
        Task<IEnumerable<ProductImageDto>> GetAllAsync();
        Task<ProductImageDto?> GetByIdAsync(Guid id);
        Task<ProductImageDto> UploadAsync(CreateProductImageDto dto, IFormFile imageFile, string createdBy);
        Task<ProductImageDto> UpdateAsync(Guid id, UpdateProductImageDto dto, string updatedBy);
        Task<bool> DeleteAsync(Guid id, string deletedBy);
        Task<IEnumerable<ProductImageDto>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<ProductImageDto>> GetByVariantIdAsync(Guid variantId);
        Task<bool> SetPrimaryAsync(Guid id, bool isPrimary, string updatedBy);
        Task<bool> ReorderAsync(List<ImageOrderItem> imagesOrder, string updatedBy);
    }
}