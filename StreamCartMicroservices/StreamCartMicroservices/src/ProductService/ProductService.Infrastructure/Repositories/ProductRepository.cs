using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Data.Repositories;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Repositories
{
    public class ProductRepository : EfCoreGenericRepository<Product>, IProductRepository
    {
        private readonly ProductContext _productContext;

        public ProductRepository(ProductContext productContext) : base(productContext)
        {
            _productContext = productContext;
        }

        public async Task<IEnumerable<Product>> GetByShopIdAsync(Guid? shopId)
        {
            if (!shopId.HasValue)
                return new List<Product>();

            return await _dbSet
                .Where(p => p.ShopId == shopId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveByCategoryIdAsync(Guid categoryId)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId && p.IsActive && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<PagedResult<Product>> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            ProductSortOption sortOption = ProductSortOption.DateCreatedDesc,
            bool activeOnly = false,
            Guid? shopId = null,
            Guid? categoryId = null)
        {
            // Build query
            var query = _dbSet.AsQueryable();

            // Filter by deleted
            query = query.Where(p => !p.IsDeleted);

            // Filter by active if needed
            if (activeOnly)
            {
                query = query.Where(p => p.IsActive);
            }

            // Filter by shop if specified
            if (shopId.HasValue)
            {
                query = query.Where(p => p.ShopId == shopId.Value);
            }

            // Filter by category if specified
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Sort data
            query = sortOption switch
            {
                ProductSortOption.NameAsc => query.OrderBy(p => p.ProductName),
                ProductSortOption.NameDesc => query.OrderByDescending(p => p.ProductName),
                ProductSortOption.PriceAsc => query.OrderBy(p => p.BasePrice),
                ProductSortOption.PriceDesc => query.OrderByDescending(p => p.BasePrice),
                ProductSortOption.DateCreatedAsc => query.OrderBy(p => p.CreatedAt),
                ProductSortOption.BestSelling => query.OrderByDescending(p => p.QuantitySold),
                _ => query.OrderByDescending(p => p.CreatedAt) // Default to DateCreatedDesc
            };

            // Calculate total count
            var totalCount = await query.CountAsync();

            // Get paged data
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return paged result
            return new PagedResult<Product>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<IEnumerable<Product>> GetBestSellingProductsAsync(
            int count = 10,
            Guid? shopId = null,
            Guid? categoryId = null)
        {
            var query = _dbSet.AsQueryable()
                .Where(p => p.IsActive && !p.IsDeleted);

            // Filter by shop if specified
            if (shopId.HasValue)
            {
                query = query.Where(p => p.ShopId == shopId.Value);
            }

            // Filter by category if specified
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            return await query
                .OrderByDescending(p => p.QuantitySold)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return true;

            var query = _dbSet.Where(p => p.SKU == sku && !p.IsDeleted);

            if (excludeProductId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProductId.Value);
            }

            return await query.CountAsync() == 0;
        }
        public async Task<IEnumerable<Product>> GetProductsHaveFlashSale()
        {
            var query = _dbSet.Where(p => p.DiscountPrice != 0);
            return await query.ToListAsync();
        }
    }
}