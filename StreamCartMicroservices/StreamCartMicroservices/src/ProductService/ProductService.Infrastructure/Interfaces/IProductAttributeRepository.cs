using ProductService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Interfaces
{
    public interface IProductAttributeRepository : IGenericRepository<ProductAttribute>
    {
        Task<bool> IsNameUniqueAsync(string name);
        Task<bool> IsNameUniqueAsync(string name, Guid attributeId);
        Task<IEnumerable<ProductAttribute>> GetAttributesByProductIdAsync(Guid productId);
        Task<bool> HasAttributeValuesAsync(Guid attributeId);
    }
}