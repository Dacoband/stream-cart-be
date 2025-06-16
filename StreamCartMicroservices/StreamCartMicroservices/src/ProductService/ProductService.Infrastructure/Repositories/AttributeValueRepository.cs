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
    public class AttributeValueRepository : EfCoreGenericRepository<AttributeValue>, IAttributeValueRepository
    {
        private readonly ProductContext _dbContext;
        private readonly DbSet<ProductCombination> _combinationSet;

        public AttributeValueRepository(ProductContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _combinationSet = _dbContext.Set<ProductCombination>();
        }

        public async Task<IEnumerable<AttributeValue>> GetByAttributeIdAsync(Guid attributeId)
        {
            return await _dbSet
                .Where(av => av.AttributeId == attributeId && !av.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsValueNameUniqueForAttributeAsync(Guid attributeId, string valueName)
        {
            if (string.IsNullOrWhiteSpace(valueName))
                return false;

            return !await _dbSet
                .AnyAsync(av => av.AttributeId == attributeId && av.ValueName == valueName && !av.IsDeleted);
        }

        public async Task<bool> IsValueNameUniqueForAttributeAsync(Guid attributeId, string valueName, Guid valueId)
        {
            if (string.IsNullOrWhiteSpace(valueName))
                return false;

            return !await _dbSet
                .AnyAsync(av => av.AttributeId == attributeId && av.ValueName == valueName
                          && av.Id != valueId && !av.IsDeleted);
        }

        public async Task<bool> IsUsedInCombinationsAsync(Guid valueId)
        {
            return await _combinationSet
                .AnyAsync(c => c.AttributeValueId == valueId);
        }
    }
}