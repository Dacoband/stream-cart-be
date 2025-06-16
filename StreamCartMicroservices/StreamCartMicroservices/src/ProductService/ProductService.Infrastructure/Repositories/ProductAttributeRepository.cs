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
    public class ProductAttributeRepository : EfCoreGenericRepository<ProductAttribute>, IProductAttributeRepository
    {
        private readonly ProductContext _dbContext;
        private readonly DbSet<ProductCombination> _combinationSet;
        private readonly DbSet<AttributeValue> _attributeValueSet;
        private readonly DbSet<ProductVariant> _variantSet;

        public ProductAttributeRepository(ProductContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _combinationSet = _dbContext.Set<ProductCombination>();
            _attributeValueSet = _dbContext.Set<AttributeValue>();
            _variantSet = _dbContext.Set<ProductVariant>();
        }

        public async Task<bool> IsNameUniqueAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return !await _dbSet
                .AnyAsync(a => a.Name == name && !a.IsDeleted);
        }

        public async Task<bool> IsNameUniqueAsync(string name, Guid attributeId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return !await _dbSet
                .AnyAsync(a => a.Name == name && a.Id != attributeId && !a.IsDeleted);
        }

        public async Task<IEnumerable<ProductAttribute>> GetAttributesByProductIdAsync(Guid productId)
        {
            var query = from variant in _variantSet
                        join combination in _combinationSet on variant.Id equals combination.VariantId
                        join attrValue in _attributeValueSet on combination.AttributeValueId equals attrValue.Id
                        join attr in _dbSet on attrValue.AttributeId equals attr.Id
                        where variant.ProductId == productId && !variant.IsDeleted && !attr.IsDeleted
                        select attr;

            return await query.Distinct().ToListAsync();
        }

        public async Task<bool> HasAttributeValuesAsync(Guid attributeId)
        {
            return await _attributeValueSet
                .AnyAsync(av => av.AttributeId == attributeId && !av.IsDeleted);
        }
    }
}