using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Common.Data.Repositories;
using Shared.Common.Domain.Bases;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using ShopService.Domain.Enums;
using ShopService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopService.Infrastructure.Repositories
{
    public class ShopVoucherRepository : EfCoreGenericRepository<ShopVoucher>, IShopVoucherRepository
    {
        private readonly ShopContext _context;
        private readonly ILogger<ShopVoucherRepository> _logger;

        public ShopVoucherRepository(ShopContext context, ILogger<ShopVoucherRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;

        }

        public async Task<ShopVoucher?> GetByCodeAsync(string code)
        {
            return await _context.Set<ShopVoucher>()
                .Include(v => v.Shop)
                .FirstOrDefaultAsync(v => v.Code == code.ToUpper() && !v.IsDeleted);
        }

        public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
        {
            var query = _context.Set<ShopVoucher>()
                .Where(v => v.Code == code.ToUpper() && !v.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(v => v.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        public async Task<IEnumerable<ShopVoucher>> GetActiveVouchersByShopAsync(Guid shopId)
        {
            var now = DateTime.UtcNow;
            return await _context.Set<ShopVoucher>()
                .Include(v => v.Shop)
                .Where(v => v.ShopId == shopId &&
                           v.IsActive &&
                           !v.IsDeleted &&
                           v.StartDate <= now &&
                           v.EndDate >= now &&
                           v.UsedQuantity < v.AvailableQuantity)
                .OrderBy(v => v.EndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShopVoucher>> GetVouchersByShopAsync(Guid shopId, bool? isActive = null)
        {
            var query = _context.Set<ShopVoucher>()
                .Include(v => v.Shop)
                .Where(v => v.ShopId == shopId && !v.IsDeleted);

            if (isActive.HasValue)
                query = query.Where(v => v.IsActive == isActive.Value);

            return await query
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<PagedResult<ShopVoucher>> GetVouchersPagedAsync(
            Guid? shopId = null,
            bool? isActive = null,
            VoucherType? type = null,
            bool? isExpired = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.Set<ShopVoucher>()
                .Include(v => v.Shop)
                .Where(v => !v.IsDeleted);

            if (shopId.HasValue)
                query = query.Where(v => v.ShopId == shopId.Value);

            if (isActive.HasValue)
                query = query.Where(v => v.IsActive == isActive.Value);

            if (type.HasValue)
                query = query.Where(v => v.Type == type.Value);

            if (isExpired.HasValue)
            {
                var now = DateTime.UtcNow;
                if (isExpired.Value)
                    query = query.Where(v => v.EndDate < now);
                else
                    query = query.Where(v => v.EndDate >= now);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ShopVoucher>
            {
                Items = items,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<ShopVoucher>> GetValidVouchersForOrderAsync(Guid? shopId, decimal orderAmount)
        {
            try
            {
                var now = DateTime.UtcNow;

                _logger.LogInformation("🎫 Getting valid vouchers for order amount: {OrderAmount}đ, ShopId: {ShopId}",
                    orderAmount, shopId?.ToString() ?? "ALL_SHOPS");

                // Tạo query cơ bản
                var query = _context.Set<ShopVoucher>()
                    .Include(v => v.Shop)
                    .Where(v => v.IsActive &&
                               !v.IsDeleted &&
                               v.StartDate <= now &&
                               v.EndDate >= now &&
                               v.UsedQuantity < v.AvailableQuantity &&
                               v.MinOrderAmount <= orderAmount);

                if (shopId.HasValue)
                {
                    query = query.Where(v => v.ShopId == shopId.Value);
                }

                var vouchers = await query.ToListAsync();

                
                var result = vouchers
                    .Select(v => new {
                        Voucher = v,
                        DiscountAmount = CalculateDiscountAmount(v, orderAmount)
                    })
                    .OrderByDescending(x => x.DiscountAmount) 
                    .ThenBy(x => x.Voucher.EndDate) 
                    .Select(x => x.Voucher)
                    .ToList();

                _logger.LogInformation("✅ Found {Count} valid vouchers for order amount {OrderAmount}đ, ShopId: {ShopId}",
                    result.Count, orderAmount, shopId?.ToString() ?? "ALL_SHOPS");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting valid vouchers for order amount {OrderAmount}, ShopId: {ShopId}",
                    orderAmount, shopId);
                throw;
            }
        }
        private decimal CalculateDiscountAmount(ShopVoucher voucher, decimal orderAmount)
        {
            decimal discountAmount = 0;

            if (voucher.Type == VoucherType.Percentage)
            {
                discountAmount = orderAmount * (voucher.Value / 100);
                if (voucher.MaxValue.HasValue && discountAmount > voucher.MaxValue.Value)
                {
                    discountAmount = voucher.MaxValue.Value;
                }
            }
            else if (voucher.Type == VoucherType.FixedAmount)
            {
                discountAmount = voucher.Value;
            }

            return discountAmount;
        }
        public async Task<int> GetUsageStatisticsAsync(Guid voucherId)
        {
            var voucher = await _context.Set<ShopVoucher>()
                .FirstOrDefaultAsync(v => v.Id == voucherId && !v.IsDeleted);

            return voucher?.UsedQuantity ?? 0;
        }
    }
}