using ProductService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Interfaces
{
    public interface IAttributeValueRepository : IGenericRepository<AttributeValue>
    {
        Task<IEnumerable<AttributeValue>> GetByAttributeIdAsync(Guid attributeId);
        Task<bool> IsValueNameUniqueForAttributeAsync(Guid attributeId, string valueName);
        Task<bool> IsValueNameUniqueForAttributeAsync(Guid attributeId, string valueName, Guid valueId);
        Task<bool> IsUsedInCombinationsAsync(Guid valueId);
    }
}