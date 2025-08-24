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

            // Sau đó lấy tất cả FlashSale của những sản phẩm đó
            return await Task.FromResult(_dbSet
                .Where(fs => shopProductIds.Contains(fs.ProductId) && !fs.IsDeleted)
                .OrderByDescending(fs => fs.CreatedAt)
                .ToList());
        }
    }
}
