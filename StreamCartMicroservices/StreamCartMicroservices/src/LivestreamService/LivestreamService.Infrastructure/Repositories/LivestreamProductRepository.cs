using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using LivestreamService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Repositories
{
    public class LivestreamProductRepository : IGenericRepository<LivestreamProduct>, ILivestreamProductRepository
    {
        private readonly ILogger<LivestreamProductRepository> _logger;
        private readonly LivestreamDbContext _context;
        private readonly IProductServiceClient _productServiceClient;

        public LivestreamProductRepository(
            LivestreamDbContext context,
            ILogger<LivestreamProductRepository> logger,
            IProductServiceClient productServiceClient)
        {
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _productServiceClient = productServiceClient;
        }

        public async Task<IEnumerable<LivestreamProduct>> GetByLivestreamIdAsync(Guid livestreamId)
        {
            try
            {
                return await _context.LivestreamProducts
                    .Where(p => p.LivestreamId == livestreamId && !p.IsDeleted)
                    .OrderByDescending(p => p.IsPin) // Sản phẩm ghim lên đầu
                    .ThenByDescending(p => p.CreatedAt) // Sau đó sắp xếp theo thời gian tạo
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream products for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        public async Task<LivestreamProduct> GetLivestreamProductAsync(Guid id)
        {
            try
            {
                return await _context.LivestreamProducts
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream product {Id}", id);
                throw;
            }
        }

        public async Task<bool> ExistsByProductInLivestreamAsync(Guid livestreamId, string productId, string variantId)
        {
            try
            {
                return await _context.LivestreamProducts
                    .AnyAsync(p => p.LivestreamId == livestreamId &&
                               p.ProductId == productId &&
                               p.VariantId == variantId &&
                               !p.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product {ProductId} with variant {VariantId} exists in livestream {LivestreamId}",
                    productId, variantId, livestreamId);
                throw;
            }
        }

        public async Task<IEnumerable<LivestreamProduct>> GetPinnedProductsAsync(Guid livestreamId, int limit = 5)
        {
            try
            {
                return await _context.LivestreamProducts
                    .Where(p => p.LivestreamId == livestreamId && p.IsPin && !p.IsDeleted)
                    .OrderByDescending(p => p.LastModifiedAt) // Sắp xếp theo thời gian cập nhật gần nhất
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pinned products for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        public async Task<IEnumerable<LivestreamProduct>> GetFlashSaleProductsAsync(Guid livestreamId)
        {
            try
            {
                // Vì FlashSaleId đã bị xóa, trả về danh sách rỗng hoặc có thể implement logic khác
                return await _context.LivestreamProducts
                    .Where(p => p.LivestreamId == livestreamId && !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flash sale products for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        public async Task<bool> UpdateDisplayOrderAsync(Guid id, int displayOrder)
        {
            try
            {
                // Vì DisplayOrder đã bị xóa, method này không còn có tác dụng
                // Có thể trả về false hoặc throw NotSupportedException
                _logger.LogWarning("UpdateDisplayOrderAsync is no longer supported as DisplayOrder field has been removed");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating display order for livestream product {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LivestreamProduct>> GetProductsOrderedByDisplayAsync(Guid livestreamId)
        {
            try
            {
                return await _context.LivestreamProducts
                    .Where(p => p.LivestreamId == livestreamId && !p.IsDeleted)
                    .OrderByDescending(p => p.IsPin) // Sản phẩm ghim lên đầu
                    .ThenByDescending(p => p.CreatedAt) // Sau đó sắp xếp theo thời gian tạo
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ordered products for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        // IGenericRepository implementation - sử dụng EF Core
        public async Task<LivestreamProduct?> GetByIdAsync(string id)
        {
            return await _context.LivestreamProducts.FindAsync(Guid.Parse(id));
        }

        public async Task<LivestreamProduct?> FindOneAsync(Expression<Func<LivestreamProduct, bool>> filter)
        {
            return await _context.LivestreamProducts.FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<LivestreamProduct>> FilterByAsync(Expression<Func<LivestreamProduct, bool>> filter)
        {
            return await _context.LivestreamProducts.Where(filter).ToListAsync();
        }

        public async Task<IEnumerable<LivestreamProduct>> GetAllAsync()
        {
            return await _context.LivestreamProducts.ToListAsync();
        }

        public async Task InsertAsync(LivestreamProduct entity)
        {
            await _context.LivestreamProducts.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task InsertManyAsync(IEnumerable<LivestreamProduct> entities)
        {
            await _context.LivestreamProducts.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task ReplaceAsync(string id, LivestreamProduct entity)
        {
            _context.LivestreamProducts.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.LivestreamProducts.FindAsync(Guid.Parse(id));
            if (entity != null)
            {
                entity.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteManyAsync(IEnumerable<string> ids)
        {
            var guidIds = ids.Select(Guid.Parse);
            var entities = await _context.LivestreamProducts
                .Where(x => guidIds.Contains(x.Id))
                .ToListAsync();

            foreach (var entity in entities)
            {
                entity.Delete();
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.LivestreamProducts.AnyAsync(x => x.Id == Guid.Parse(id));
        }

        public async Task<bool> ExistsAsync(Expression<Func<LivestreamProduct, bool>> filter)
        {
            return await _context.LivestreamProducts.AnyAsync(filter);
        }

        public async Task<int> CountAsync(Expression<Func<LivestreamProduct, bool>> filter)
        {
            return await _context.LivestreamProducts.CountAsync(filter);
        }

        public async Task<int> CountAsync()
        {
            return await _context.LivestreamProducts.CountAsync();
        }

        public Task<PagedResult<LivestreamProduct>> SearchAsync(string searchTerm, PaginationParams paginationParams, string[]? searchableFields = null, Expression<Func<LivestreamProduct, bool>>? filter = null, bool exactMatch = false)
        {
            throw new NotImplementedException("Search not implemented for LivestreamProduct");
        }
        public async Task<LivestreamProduct?> GetByCompositeKeyAsync(Guid livestreamId, string productId, string variantId)
        {
            try
            {
                return await _context.LivestreamProducts
                    .FirstOrDefaultAsync(p => p.LivestreamId == livestreamId &&
                                           p.ProductId == productId &&
                                           p.VariantId == variantId &&
                                           !p.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream product for LivestreamId: {LivestreamId}, ProductId: {ProductId}, VariantId: {VariantId}",
                    livestreamId, productId, variantId);
                throw;
            }
        }
        public async Task<LivestreamProduct?> GetCurrentPinnedProductAsync(Guid livestreamId)
        {
            try
            {
                return await _context.LivestreamProducts
                    .FirstOrDefaultAsync(p => p.LivestreamId == livestreamId && p.IsPin && !p.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current pinned product for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }

        // ✅ NEW METHOD: Get all pinned products in a livestream
        public async Task<IEnumerable<LivestreamProduct>> GetAllPinnedProductsByLivestreamAsync(Guid livestreamId)
        {
            try
            {
                return await _context.LivestreamProducts
                    .Where(p => p.LivestreamId == livestreamId && p.IsPin && !p.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all pinned products for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }
        public async Task UnpinAllProductsInLivestreamAsync(Guid livestreamId, string modifiedBy)
        {
            try
            {
                var pinnedProducts = await _context.LivestreamProducts
                    .Where(p => p.LivestreamId == livestreamId && p.IsPin && !p.IsDeleted)
                    .ToListAsync();

                foreach (var product in pinnedProducts)
                {
                    product.SetPin(false, modifiedBy);
                }

                if (pinnedProducts.Any())
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Unpinned {Count} products in livestream {LivestreamId}",
                        pinnedProducts.Count, livestreamId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpinning all products for livestream {LivestreamId}", livestreamId);
                throw;
            }
        }
        // Thêm các methods này vào class LivestreamProductRepository

        /// <summary>
        /// Lấy sản phẩm trong livestream theo SKU
        /// </summary>
        public async Task<LivestreamProduct?> GetBySkuInLivestreamAsync(Guid livestreamId, string sku)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sku))
                    return null;

                _logger.LogInformation("Searching for product with SKU {Sku} in livestream {LivestreamId}", sku, livestreamId);

                // Lấy tất cả sản phẩm trong livestream
                var livestreamProducts = await GetByLivestreamIdAsync(livestreamId);

                foreach (var lsProduct in livestreamProducts)
                {
                    try
                    {
                        // Lấy thông tin sản phẩm từ Product Service để check SKU
                        var productInfo = await _productServiceClient.GetProductByIdAsync(lsProduct.ProductId);

                        if (productInfo?.SKU?.Equals(sku, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            // Nếu không có variant, return luôn
                            if (string.IsNullOrEmpty(lsProduct.VariantId))
                            {
                                _logger.LogInformation("Found product with SKU {Sku} (base product) in livestream {LivestreamId}", sku, livestreamId);
                                return lsProduct;
                            }
                        }

                        // Kiểm tra SKU của variant nếu có
                        if (!string.IsNullOrEmpty(lsProduct.VariantId))
                        {
                            try
                            {
                                var variantInfo = await _productServiceClient.GetProductVariantAsync(lsProduct.ProductId, lsProduct.VariantId);
                                if (variantInfo?.SKU?.Equals(sku, StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    _logger.LogInformation("Found product with SKU {Sku} (variant) in livestream {LivestreamId}", sku, livestreamId);
                                    return lsProduct;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to get variant {VariantId} info for SKU search", lsProduct.VariantId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get product {ProductId} info for SKU search", lsProduct.ProductId);
                    }
                }

                _logger.LogInformation("No product found with SKU {Sku} in livestream {LivestreamId}", sku, livestreamId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream product by SKU {Sku} in livestream {LivestreamId}", sku, livestreamId);
                throw;
            }
        }

        /// <summary>
        /// Lấy nhiều sản phẩm theo danh sách SKU
        /// </summary>
        public async Task<IEnumerable<LivestreamProduct>> GetBySkusInLivestreamAsync(Guid livestreamId, IEnumerable<string> skus)
        {
            try
            {
                if (skus == null || !skus.Any())
                    return new List<LivestreamProduct>();

                _logger.LogInformation("Searching for products with SKUs [{Skus}] in livestream {LivestreamId}",
                    string.Join(", ", skus), livestreamId);

                var results = new List<LivestreamProduct>();

                foreach (var sku in skus.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    var product = await GetBySkuInLivestreamAsync(livestreamId, sku);
                    if (product != null)
                    {
                        results.Add(product);
                    }
                }

                _logger.LogInformation("Found {Count} products out of {Total} SKUs in livestream {LivestreamId}",
                    results.Count, skus.Count(), livestreamId);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream products by SKUs in livestream {LivestreamId}", livestreamId);
                throw;
            }
        }
    }
}