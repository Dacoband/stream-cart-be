using ProductService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Interfaces
{
    public interface IProductVariantRepository : IGenericRepository<ProductVariant>
    {
        Task<IEnumerable<ProductVariant>> GetByProductIdAsync(Guid productId);
        Task<bool> IsSkuUniqueAsync(string sku);
        Task<bool> IsSkuUniqueAsync(string sku, Guid variantId);
        Task<bool> ExistsByProductIdAsync(Guid productId);
        Task<bool> BulkUpdateStockAsync(Dictionary<Guid, int> stockUpdates);
    }
}