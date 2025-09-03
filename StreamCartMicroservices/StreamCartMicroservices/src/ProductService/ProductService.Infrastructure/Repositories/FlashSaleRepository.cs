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
            var now = DateTime.UtcNow;

            var occupiedSlots = await _dbSet
                .Where(fs => !fs.IsDeleted &&
                           fs.StartTime >= dayStart &&
                           fs.StartTime <= dayEnd)
                .Select(fs => fs.Slot)
                .Distinct()
                .ToListAsync();

            var slotsToExclude = new List<int>(occupiedSlots);

            if (date.Date == DateTime.UtcNow.Date)
            {
                // Convert UTC hiện tại sang SE Asia timezone để so sánh với slots
                var seAsiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var localNow = TimeZoneInfo.ConvertTimeFromUtc(now, seAsiaTimeZone);
                var currentTimeOfDay = localNow.TimeOfDay;

                // ✅ FIX: Loại bỏ cả slot đã kết thúc VÀ slot đang diễn ra
                var unavailableSlots = FlashSaleSlotHelper.SlotTimeRanges
                    .Where(slot => slot.Value.Start <= currentTimeOfDay) // ✅ LOẠI BỎ CÁC SLOT ĐÃ BẮT ĐẦU (bao gồm đang diễn ra)
                    .Select(slot => slot.Key)
                    .ToList();

                // Thêm unavailable slots vào danh sách loại bỏ
                slotsToExclude.AddRange(unavailableSlots);
                slotsToExclude = slotsToExclude.Distinct().ToList();
            }

            return FlashSaleSlotHelper.GetAvailableSlotsForDate(date, slotsToExclude);
        }

        //public async Task<List<Guid>> GetProductsWithoutFlashSaleAsync(Guid shopId, DateTime startTime, DateTime endTime)
        //{
        //    var utcStartTime = startTime.Kind == DateTimeKind.Utc ? startTime : DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
        //    var utcEndTime = endTime.Kind == DateTimeKind.Utc ? endTime : DateTime.SpecifyKind(endTime, DateTimeKind.Utc);

        //    var shopProductIds = await _dbContext.Products
        //        .Where(p => p.ShopId == shopId && !p.IsDeleted && p.IsActive)
        //        .Select(p => p.Id)
        //        .ToListAsync();

        //    var productsWithFlashSale = await _dbSet
        //        .Where(fs => !fs.IsDeleted &&
        //                   shopProductIds.Contains(fs.ProductId) &&
        //                   ((fs.StartTime <= utcStartTime && fs.EndTime >= utcStartTime) ||
        //                    (fs.StartTime <= utcEndTime && fs.EndTime >= utcEndTime) ||
        //                    (fs.StartTime >= utcStartTime && fs.EndTime <= utcEndTime)))
        //        .Select(fs => fs.ProductId)
        //        .Distinct()
        //        .ToListAsync();

        //    return shopProductIds.Except(productsWithFlashSale).ToList();
        //}
        public async Task<List<Guid>> GetProductsWithoutFlashSaleAsync(Guid shopId, DateTime startTime, DateTime endTime)
        {
            var utcStartTime = startTime.Kind == DateTimeKind.Utc ? startTime : DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
            var utcEndTime = endTime.Kind == DateTimeKind.Utc ? endTime : DateTime.SpecifyKind(endTime, DateTimeKind.Utc);

            var shopProductIds = await _dbContext.Products
                .Where(p => p.ShopId == shopId && !p.IsDeleted && p.IsActive)
                .Select(p => p.Id)
                .ToListAsync();

            return shopProductIds;
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
        public async Task<List<FlashSale>> GetFlashSalesBySlotAndDateAsync(Guid shopId, DateTime date, int slot)
        {
            var dayStart = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var dayEnd = DateTime.SpecifyKind(date.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var shopProductIds = await _dbContext.Products
                .Where(p => p.ShopId == shopId && !p.IsDeleted)
                .Select(p => p.Id)
                .ToListAsync();

            return await _dbSet
                .Where(fs => !fs.IsDeleted &&
                           fs.Slot == slot &&
                           fs.StartTime >= dayStart &&
                           fs.StartTime <= dayEnd &&
                           shopProductIds.Contains(fs.ProductId))
                .OrderByDescending(fs => fs.CreatedAt)
                .ToListAsync();
        }
        public async Task<List<int>> GetAvailableSlotsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate.Date == DateTime.UtcNow.Date)
                {
                    return await GetAvailableSlotsAsync(startDate); 
                }

                var occupiedSlots = await _dbSet
                    .Where(fs => !fs.IsDeleted &&
                               fs.StartTime < endDate &&
                               fs.EndTime > startDate)
                    .Select(fs => fs.Slot)
                    .Distinct()
                    .ToListAsync();

                var allSlots = Enumerable.Range(1, 8).ToList();
                return allSlots.Except(occupiedSlots).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting available slots for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}: {ex.Message}", ex);
            }
        }
    }
}
