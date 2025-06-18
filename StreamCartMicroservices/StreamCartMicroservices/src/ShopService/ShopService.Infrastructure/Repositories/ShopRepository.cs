using Microsoft.EntityFrameworkCore;
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
    public class ShopRepository : EfCoreGenericRepository<Shop>, IShopRepository
    {
        private readonly ShopContext _context;

        public ShopRepository(ShopContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Shop?> GetByIdForAccountAsync(Guid shopId, Guid accountId)
        {
            // Lấy shop
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.Id == shopId && !s.IsDeleted);
            if (shop == null)
                return null;

            // Kiểm tra account có thuộc về shop này không bằng EF Core thay vì SQL raw
            var accountBelongsToShop = await _context.Database
                .ExecuteSqlRawAsync(
                    "SELECT COUNT(1) FROM accounts WHERE id = {0} AND shop_id = {1} AND is_deleted = false",
                    accountId, shopId) > 0;

            return accountBelongsToShop ? shop : null;
        }
        public async Task<IEnumerable<Shop>> GetByApprovalStatusAsync(ApprovalStatus status)
        {
            return await _context.Shops
                .Where(s => s.ApprovalStatus == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Shop>> GetByStatusAsync(ShopStatus status)
        {
            return await _context.Shops
                .Where(s => s.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Shop>> GetTopRatedShopsAsync(int count)
        {
            return await _context.Shops
                .Where(s => s.Status == ShopStatus.Active && s.ApprovalStatus == ApprovalStatus.Approved)
                .OrderByDescending(s => s.RatingAverage)
                .ThenByDescending(s => s.TotalReview)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Shops.AsQueryable();
            
            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }
            
            return !await query.AnyAsync(s => s.ShopName.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Shop>> SearchByNameAsync(string nameQuery)
        {
            if (string.IsNullOrWhiteSpace(nameQuery))
                return Enumerable.Empty<Shop>();
                
            return await _context.Shops
                .Where(s => s.Status == ShopStatus.Active && 
                           s.ApprovalStatus == ApprovalStatus.Approved &&
                           EF.Functions.ILike(s.ShopName, $"%{nameQuery}%"))
                .ToListAsync();
        }
        public async Task<IEnumerable<Shop>> GetShopsByAccountIdAsync(Guid accountId)
        {
            var shops = await _context.Database
                .SqlQueryRaw<Shop>(@"
            SELECT s.* FROM shops s
            INNER JOIN accounts a ON a.shop_id = s.id
            WHERE a.id = {0} AND s.is_deleted = false AND a.is_deleted = false",
                    accountId)
                .ToListAsync();

            return shops;
        }
        public async Task<PagedResult<Shop>> GetPagedShopsAsync(
            int pageNumber, 
            int pageSize, 
            ShopStatus? status = null, 
            ApprovalStatus? approvalStatus = null, 
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true)
        {
            // Start with all shops
            IQueryable<Shop> query = _context.Shops;
            
            // Apply filters
            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }
            
            if (approvalStatus.HasValue)
            {
                query = query.Where(s => s.ApprovalStatus == approvalStatus.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => EF.Functions.ILike(s.ShopName, $"%{searchTerm}%") ||
                                        EF.Functions.ILike(s.Description, $"%{searchTerm}%"));
            }
            
            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = ascending 
                            ? query.OrderBy(s => s.ShopName) 
                            : query.OrderByDescending(s => s.ShopName);
                        break;
                    case "rating":
                        query = ascending 
                            ? query.OrderBy(s => s.RatingAverage) 
                            : query.OrderByDescending(s => s.RatingAverage);
                        break;
                    case "date":
                        query = ascending 
                            ? query.OrderBy(s => s.RegistrationDate) 
                            : query.OrderByDescending(s => s.RegistrationDate);
                        break;
                    case "products":
                        query = ascending 
                            ? query.OrderBy(s => s.TotalProduct) 
                            : query.OrderByDescending(s => s.TotalProduct);
                        break;
                    default:
                        query = query.OrderByDescending(s => s.CreatedAt);
                        break;
                }
            }
            else
            {
                // Default sorting
                query = query.OrderByDescending(s => s.CreatedAt);
            }
            
            // Calculate total count for pagination
            var totalCount = await query.CountAsync();
            
            // Apply pagination
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return new PagedResult<Shop>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
