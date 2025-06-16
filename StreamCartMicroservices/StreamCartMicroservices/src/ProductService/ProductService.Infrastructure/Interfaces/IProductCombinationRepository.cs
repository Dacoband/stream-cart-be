using ProductService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Interfaces
{
    public interface IProductCombinationRepository : IGenericRepository<ProductCombination>
    {
        Task<IEnumerable<ProductCombination>> GetByVariantIdAsync(Guid variantId);
        Task<IEnumerable<ProductCombination>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<ProductCombination>> GetCombinationsByVariantIdAsync(Guid variantId);
        Task<bool> ExistsByVariantIdAndAttributeValueIdAsync(Guid variantId, Guid attributeValueId);
        Task<bool> DeleteByVariantIdAsync(Guid variantId);
        Task<bool> DeleteByVariantIdAndAttributeValueIdAsync(Guid variantId, Guid attributeValueId);
    }
}