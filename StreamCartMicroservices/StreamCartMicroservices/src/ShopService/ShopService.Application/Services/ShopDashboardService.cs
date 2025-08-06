using MediatR;
using ShopService.Application.Commands.Dashboard;
using ShopService.Application.DTOs.Dashboard;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries.Dashboard;
using System;
using System.Threading.Tasks;

namespace ShopService.Application.Services
{
    public class ShopDashboardService : IShopDashboardService
    {
        private readonly IMediator _mediator;

        public ShopDashboardService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ShopDashboardDTO> GetDashboardAsync(Guid shopId, DateTime? fromDate, DateTime? toDate, string periodType)
        {
            var query = new GetShopDashboardQuery
            {
                ShopId = shopId,
                FromDate = fromDate,
                ToDate = toDate,
                PeriodType = periodType
            };

            return await _mediator.Send(query);
        }

        public async Task<ShopDashboardSummaryDTO> GetDashboardSummaryAsync(Guid shopId)
        {
            var query = new GetShopDashboardSummaryQuery { ShopId = shopId };
            return await _mediator.Send(query);
        }

        public async Task<ShopDashboardDTO> GenerateDashboardAsync(Guid shopId, DateTime fromDate, DateTime toDate, string periodType, string generatedBy)
        {
            var command = new GenerateDashboardCommand
            {
                ShopId = shopId,
                FromDate = fromDate,
                ToDate = toDate,
                PeriodType = periodType,
                GeneratedBy = generatedBy
            };

            return await _mediator.Send(command);
        }

        public async Task<ShopDashboardDTO> UpdateDashboardNotesAsync(Guid dashboardId, string notes, string updatedBy)
        {
            var command = new UpdateDashboardNotesCommand
            {
                DashboardId = dashboardId,
                Notes = notes,
                UpdatedBy = updatedBy
            };

            return await _mediator.Send(command);
        }
    }
}