using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;
using ShopService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IShopDashboardRepository : IGenericRepository<ShopDashboard>
    {
        Task<ShopDashboard?> GetLatestDashboardAsync(Guid shopId, string periodType);
        Task<ShopDashboard?> GetDashboardByPeriodAsync(Guid shopId, DateTime fromDate, DateTime toDate, string periodType);
        Task<IEnumerable<ShopDashboard>> GetDashboardHistoryAsync(Guid shopId, DateTime fromDate, DateTime toDate, string periodType, int limit = 10);
        Task<PagedResult<ShopDashboard>> GetPagedDashboardsAsync(Guid shopId, int pageNumber, int pageSize, string? periodType = null);
    }
}
