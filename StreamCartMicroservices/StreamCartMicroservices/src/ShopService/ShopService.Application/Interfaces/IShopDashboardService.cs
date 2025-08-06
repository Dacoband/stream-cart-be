using ShopService.Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IShopDashboardService
    {
        Task<ShopDashboardDTO> GetDashboardAsync(Guid shopId, DateTime? fromDate, DateTime? toDate, string periodType);
        Task<ShopDashboardSummaryDTO> GetDashboardSummaryAsync(Guid shopId);
        Task<ShopDashboardDTO> GenerateDashboardAsync(Guid shopId, DateTime fromDate, DateTime toDate, string periodType, string generatedBy);
        Task<ShopDashboardDTO> UpdateDashboardNotesAsync(Guid dashboardId, string notes, string updatedBy);
    }
}
