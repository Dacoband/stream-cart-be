using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;
using ShopService.Domain.Entities;
using ShopService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IShopVoucherRepository : IGenericRepository<ShopVoucher>
    {
        Task<ShopVoucher?> GetByCodeAsync(string code);
        Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
        Task<IEnumerable<ShopVoucher>> GetActiveVouchersByShopAsync(Guid shopId);
        Task<IEnumerable<ShopVoucher>> GetVouchersByShopAsync(Guid shopId, bool? isActive = null);
        Task<PagedResult<ShopVoucher>> GetVouchersPagedAsync(
            Guid? shopId = null,
            bool? isActive = null,
            VoucherType? type = null,
            bool? isExpired = null,
            int pageNumber = 1,
            int pageSize = 10);
        Task<IEnumerable<ShopVoucher>> GetValidVouchersForOrderAsync(Guid shopId, decimal orderAmount);
        Task<int> GetUsageStatisticsAsync(Guid voucherId);
    }
}
