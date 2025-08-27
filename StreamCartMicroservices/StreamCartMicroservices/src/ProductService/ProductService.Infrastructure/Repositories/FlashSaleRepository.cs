using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Repositories
{
    public class FlashSaleRepository : EfCoreGenericRepository<FlashSale>, IFlashSaleRepository
    {
        private readonly ProductContext _dbContext;
        private readonly DbSet<FlashSale> _flashSaleSet;

        public FlashSaleRepository(ProductContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _flashSaleSet = _dbContext.Set<FlashSale>();
        }

        public async Task<List<FlashSale>> GetAllActiveFlashSalesAsync()
        {
            try {
                return await _dbSet.Where(x => x.IsDeleted == false).ToListAsync();
            } catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<FlashSale>> GetByTimeAndProduct(DateTime startTime, DateTime endTime, Guid productId, Guid? variantId)
        {
           var flashSlae= await _dbSet.Where(x => x.StartTime >= startTime && x.EndTime <= endTime && x.ProductId == productId && x.IsDeleted == false).ToListAsync();
            if (variantId.HasValue) {
            
            flashSlae = flashSlae.Where(x=> x.VariantId == variantId).ToList();
            }
            return flashSlae;
        }
        public async Task<List<FlashSale>> GetByShopIdAsync(Guid shopId)
        {
            var shopProductIds = _dbContext.Products
                .Where(p => p.ShopId == shopId && !p.IsDeleted)
                .Select(p => p.Id)
                .ToList();

            return await Task.FromResult(_dbSet
                .Where(fs => shopProductIds.Contains(fs.ProductId) && !fs.IsDeleted)
                .OrderByDescending(fs => fs.CreatedAt)
                .ToList());
        }
        public async Task<List<int>> GetAvailableSlotsAsync(DateTime startTime, DateTime endTime)
        {
            var occupiedSlots = await _dbSet
                .Where(fs => !fs.IsDeleted &&
                           ((fs.StartTime <= startTime && fs.EndTime >= startTime) ||
                            (fs.StartTime <= endTime && fs.EndTime >= endTime) ||
                            (fs.StartTime >= startTime && fs.EndTime <= endTime)))
                .Select(fs => fs.Slot)
                .Distinct()
                .ToListAsync();

            var allSlots = Enumerable.Range(1, 8).ToList();
            return allSlots.Except(occupiedSlots).ToList();
        }
        public async Task<List<Guid>> GetProductsWithoutFlashSaleAsync(Guid shopId, DateTime startTime, DateTime endTime)
        {
            var shopProductIds = await _dbContext.Products
                .Where(p => p.ShopId == shopId && !p.IsDeleted && p.IsActive)
                .Select(p => p.Id)
                .ToListAsync();

            var productsWithFlashSale = await _dbSet
                .Where(fs => !fs.IsDeleted &&
                           shopProductIds.Contains(fs.ProductId) &&
                           ((fs.StartTime <= startTime && fs.EndTime >= startTime) ||
                            (fs.StartTime <= endTime && fs.EndTime >= endTime) ||
                            (fs.StartTime >= startTime && fs.EndTime <= endTime)))
                .Select(fs => fs.ProductId)
                .Distinct()
                .ToListAsync();

            return shopProductIds.Except(productsWithFlashSale).ToList();
        }

        public async Task<bool> IsSlotAvailableAsync(int slot, DateTime startTime, DateTime endTime, Guid? excludeFlashSaleId = null)
        {
            var query = _dbSet.Where(fs => !fs.IsDeleted &&
                                          fs.Slot == slot &&
                                          ((fs.StartTime <= startTime && fs.EndTime >= startTime) ||
                                           (fs.StartTime <= endTime && fs.EndTime >= endTime) ||
                                           (fs.StartTime >= startTime && fs.EndTime <= endTime)));

            if (excludeFlashSaleId.HasValue)
            {
                query = query.Where(fs => fs.Id != excludeFlashSaleId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}
