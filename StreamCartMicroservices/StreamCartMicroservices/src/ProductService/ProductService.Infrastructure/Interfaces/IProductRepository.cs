using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using Shared.Common.Data.Interfaces;
using Shared.Common.Data.Repositories;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetByShopIdAsync(Guid? shopId);

        Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId);

        Task<IEnumerable<Product>> GetActiveByCategoryIdAsync(Guid categoryId);

        Task<PagedResult<Product>> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            ProductSortOption sortOption = ProductSortOption.DateCreatedDesc,
            bool activeOnly = false,
            Guid? shopId = null,
            Guid? categoryId = null);

        Task<IEnumerable<Product>> GetBestSellingProductsAsync(
            int count = 10,
            Guid? shopId = null,
            Guid? categoryId = null);

        Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null);
        Task<IEnumerable<Product>> GetProductsHaveFlashSale();
    }
}