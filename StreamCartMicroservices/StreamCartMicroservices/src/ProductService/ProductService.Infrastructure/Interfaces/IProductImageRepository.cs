using ProductService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Interfaces
{
    public interface IProductImageRepository : IGenericRepository<ProductImage>
    {
        Task<IEnumerable<ProductImage>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<ProductImage>> GetByVariantIdAsync(Guid variantId);
        Task<bool> SetPrimaryImageAsync(Guid imageId, Guid productId, Guid? variantId);
        Task<bool> UpdateDisplayOrderAsync(IEnumerable<(Guid ImageId, int DisplayOrder)> orderUpdates);
        Task<ProductImage> GetPrimaryImageAsync(Guid productId, Guid? variantId = null);
    }
}