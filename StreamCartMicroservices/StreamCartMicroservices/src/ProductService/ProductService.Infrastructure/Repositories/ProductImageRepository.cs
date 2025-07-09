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
    public class ProductImageRepository : EfCoreGenericRepository<ProductImage>, IProductImageRepository
    {
        private readonly ProductContext _productContext;

        public ProductImageRepository(ProductContext productContext) : base(productContext)
        {
            _productContext = productContext;
        }

        public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(Guid productId)
        {
            return await _dbSet
                .Where(i => i.ProductId == productId && !i.IsDeleted)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductImage>> GetByVariantIdAsync(Guid variantId)
        {
            return await _dbSet
                .Where(i => i.VariantId == variantId && !i.IsDeleted)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public async Task<ProductImage> GetPrimaryImageAsync(Guid productId, Guid? variantId = null)
        {
            if (variantId.HasValue)
            {
                return await _dbSet
                    .FirstOrDefaultAsync(i => i.ProductId == productId && i.VariantId == variantId && i.IsPrimary && !i.IsDeleted);
            }
            else
            {
                return await _dbSet
                    .FirstOrDefaultAsync(i => i.ProductId == productId && i.IsPrimary && !i.IsDeleted);
            }
        }

        public async Task<bool> SetPrimaryImageAsync(Guid imageId, Guid productId, Guid? variantId)
        {
            using var transaction = await _productContext.Database.BeginTransactionAsync();
            try
            {
                // First, reset all primary images for this product/variant
                var imagesToReset = await _dbSet
                    .Where(i => i.ProductId == productId &&
                           (variantId.HasValue ? i.VariantId == variantId : i.VariantId == null) &&
                           i.IsPrimary &&
                           !i.IsDeleted)
                    .ToListAsync();

                foreach (var image in imagesToReset)
                {
                    image.SetPrimary(false);
                    _productContext.Entry(image).State = EntityState.Modified;
                }

                // If imageId is empty, we just wanted to clear primary status
                if (imageId != Guid.Empty)
                {
                    // Set the new primary image
                    var newPrimaryImage = await _dbSet
                        .FirstOrDefaultAsync(i => i.Id == imageId && !i.IsDeleted);

                    if (newPrimaryImage == null)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    newPrimaryImage.SetPrimary(true);
                    _productContext.Entry(newPrimaryImage).State = EntityState.Modified;
                }

                await _productContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> UpdateDisplayOrderAsync(IEnumerable<(Guid ImageId, int DisplayOrder)> orderUpdates)
        {
            using var transaction = await _productContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var (imageId, displayOrder) in orderUpdates)
                {
                    var image = await _dbSet.FirstOrDefaultAsync(i => i.Id == imageId && !i.IsDeleted);
                    if (image != null)
                    {
                        image.UpdateDisplayOrder(displayOrder);
                        _productContext.Entry(image).State = EntityState.Modified;
                    }
                }

                await _productContext.SaveChangesAsync();
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