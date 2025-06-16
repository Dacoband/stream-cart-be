using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Repositories
{
    public class ProductVariantRepository : EfCoreGenericRepository<ProductVariant>, IProductVariantRepository
    {
        private readonly ProductContext _dbContext;

        public ProductVariantRepository(ProductContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(Guid productId)
        {
            return await _dbSet
                .Where(v => v.ProductId == productId && !v.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsSkuUniqueAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return true;

            return !await _dbSet
                .AnyAsync(v => v.SKU == sku && !v.IsDeleted);
        }

        public async Task<bool> IsSkuUniqueAsync(string sku, Guid variantId)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return true;

            return !await _dbSet
                .AnyAsync(v => v.SKU == sku && v.Id != variantId && !v.IsDeleted);
        }

        public async Task<bool> ExistsByProductIdAsync(Guid productId)
        {
            return await _dbSet
                .AnyAsync(v => v.ProductId == productId && !v.IsDeleted);
        }

        public async Task<bool> BulkUpdateStockAsync(Dictionary<Guid, int> stockUpdates)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                foreach (var update in stockUpdates)
                {
                    var variant = await _dbSet.FirstOrDefaultAsync(v => v.Id == update.Key && !v.IsDeleted);

                    if (variant != null)
                    {
                        variant.UpdateStock(update.Value);
                        _dbContext.Entry(variant).State = EntityState.Modified;
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}