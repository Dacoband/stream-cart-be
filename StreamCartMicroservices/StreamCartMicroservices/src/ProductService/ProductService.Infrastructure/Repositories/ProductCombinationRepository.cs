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
    public class ProductCombinationRepository : EfCoreGenericRepository<ProductCombination>, IProductCombinationRepository
    {
        private readonly ProductContext _dbContext;
        private readonly DbSet<ProductVariant> _variantSet;

        public ProductCombinationRepository(ProductContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _variantSet = _dbContext.Set<ProductVariant>();
        }

        public async Task<IEnumerable<ProductCombination>> GetByVariantIdAsync(Guid variantId)
        {
            return await _dbSet
                .Where(c => c.VariantId == variantId && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductCombination>> GetByProductIdAsync(Guid productId)
        {
            var query = from variant in _variantSet
                        join combination in _dbSet on variant.Id equals combination.VariantId
                        where variant.ProductId == productId && !variant.IsDeleted && !combination.IsDeleted
                        select combination;

            return await query.ToListAsync();
        }

        public async Task<bool> ExistsByVariantIdAndAttributeValueIdAsync(Guid variantId, Guid attributeValueId)
        {
            return await _dbSet
                .AnyAsync(c => c.VariantId == variantId && c.AttributeValueId == attributeValueId && !c.IsDeleted);
        }

        public async Task<bool> DeleteByVariantIdAsync(Guid variantId)
        {
            var combinations = await _dbSet
                .Where(c => c.VariantId == variantId)
                .ToListAsync();

            if (!combinations.Any())
                return true;

            foreach (var combination in combinations)
            {
                combination.Delete();
                _dbContext.Entry(combination).State = EntityState.Modified;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByVariantIdAndAttributeValueIdAsync(Guid variantId, Guid attributeValueId)
        {
            var combination = await _dbSet
                .FirstOrDefaultAsync(c => c.VariantId == variantId && c.AttributeValueId == attributeValueId && !c.IsDeleted);

            if (combination == null)
                return false;

            combination.Delete();
            _dbContext.Entry(combination).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<ProductCombination>> GetCombinationsByVariantIdAsync(Guid variantId)
        {
            return await _dbSet
                .Where(c => c.VariantId == variantId && !c.IsDeleted)
                .ToListAsync();
        }
    }
}