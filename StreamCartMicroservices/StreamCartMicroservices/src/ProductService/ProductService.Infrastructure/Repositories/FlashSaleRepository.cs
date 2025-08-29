using Microsoft.EntityFrameworkCore;
using ProductService.Application.Helpers;
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
            var flashSales = await _dbSet
                .Where(x => x.StartTime < endTime && x.EndTime > startTime &&
                           x.ProductId == productId && x.IsDeleted == false)
                .ToListAsync();

            if (variantId.HasValue)
            {
                flashSales = flashSales.Where(x => x.VariantId == variantId).ToList();
            }
            else
            {
                flashSales = flashSales.Where(x => !x.VariantId.HasValue).ToList();
            }

            return flashSales;
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
        public async Task<List<int>> GetAvailableSlotsAsync(DateTime date)
        {
            var dayStart = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var dayEnd = DateTime.SpecifyKind(date.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var occupiedSlots = await _dbSet
                .Where(fs => !fs.IsDeleted &&
                           fs.StartTime >= dayStart &&
                           fs.StartTime <= dayEnd)
                .Select(fs => fs.Slot)
                .Distinct()
                .ToListAsync();

            return FlashSaleSlotHelper.GetAvailableSlotsForDate(date, occupiedSlots);
        }
        public async Task<List<Guid>> GetProductsWithoutFlashSaleAsync(Guid shopId, DateTime startTime, DateTime endTime)
        {
            var utcStartTime = startTime.Kind == DateTimeKind.Utc ? startTime : DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
            var utcEndTime = endTime.Kind == DateTimeKind.Utc ? endTime : DateTime.SpecifyKind(endTime, DateTimeKind.Utc);

            var shopProductIds = await _dbContext.Products
                .Where(p => p.ShopId == shopId && !p.IsDeleted && p.IsActive)
                .Select(p => p.Id)
                .ToListAsync();

            var productsWithFlashSale = await _dbSet
                .Where(fs => !fs.IsDeleted &&
                           shopProductIds.Contains(fs.ProductId) &&
                           ((fs.StartTime <= utcStartTime && fs.EndTime >= utcStartTime) ||
                            (fs.StartTime <= utcEndTime && fs.EndTime >= utcEndTime) ||
                            (fs.StartTime >= utcStartTime && fs.EndTime <= utcEndTime)))
                .Select(fs => fs.ProductId)
                .Distinct()
                .ToListAsync();

            return shopProductIds.Except(productsWithFlashSale).ToList();
        }


        public async Task<bool> IsSlotAvailableAsync(int slot, DateTime startTime, DateTime endTime, Guid? excludeFlashSaleId = null)
        {
            var utcStartTime = startTime.Kind == DateTimeKind.Utc ? startTime : DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
            var utcEndTime = endTime.Kind == DateTimeKind.Utc ? endTime : DateTime.SpecifyKind(endTime, DateTimeKind.Utc);

            var query = _dbSet.Where(fs => !fs.IsDeleted &&
                                          fs.Slot == slot &&
                                          ((fs.StartTime <= utcStartTime && fs.EndTime >= utcStartTime) ||
                                           (fs.StartTime <= utcEndTime && fs.EndTime >= utcEndTime) ||
                                           (fs.StartTime >= utcStartTime && fs.EndTime <= utcEndTime)));

            if (excludeFlashSaleId.HasValue)
            {
                query = query.Where(fs => fs.Id != excludeFlashSaleId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}
