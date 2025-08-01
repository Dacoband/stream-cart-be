﻿using Microsoft.EntityFrameworkCore;
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

        public ShopVoucherRepository(ShopContext context) : base(context)
        {
            _context = context;
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

        public async Task<IEnumerable<ShopVoucher>> GetValidVouchersForOrderAsync(Guid shopId, decimal orderAmount)
        {
            var now = DateTime.UtcNow;
            return await _context.Set<ShopVoucher>()
                .Include(v => v.Shop)
                .Where(v => v.ShopId == shopId &&
                           v.IsActive &&
                           !v.IsDeleted &&
                           v.StartDate <= now &&
                           v.EndDate >= now &&
                           v.UsedQuantity < v.AvailableQuantity &&
                           v.MinOrderAmount <= orderAmount)
                .OrderByDescending(v => v.Value)
                .ToListAsync();
        }

        public async Task<int> GetUsageStatisticsAsync(Guid voucherId)
        {
            var voucher = await _context.Set<ShopVoucher>()
                .FirstOrDefaultAsync(v => v.Id == voucherId && !v.IsDeleted);

            return voucher?.UsedQuantity ?? 0;
        }
    }
}