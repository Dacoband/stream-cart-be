using ProductService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Interfaces
{
    public interface IFlashSaleRepository : IGenericRepository<FlashSale>
    {
        public Task<List<FlashSale>> GetByTimeAndProduct(DateTime startTime, DateTime endTime, Guid productId, Guid? variantId
            );
        public Task<List<FlashSale>> GetAllActiveFlashSalesAsync();

        Task<List<FlashSale>> GetByShopIdAsync(Guid shopId);
        Task<List<int>> GetAvailableSlotsAsync(DateTime date);
        Task<bool> IsSlotAvailableAsync(int slot, DateTime startTime, DateTime endTime, Guid? excludeFlashSaleId = null);
        Task<List<Guid>> GetProductsWithoutFlashSaleAsync(Guid shopId, DateTime startTime, DateTime endTime);
    }
}
