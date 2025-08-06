using MediatR;
using ShopService.Application.Commands.Dashboard;
using ShopService.Application.DTOs.Dashboard;
using ShopService.Application.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Dashboard
{
    public class UpdateDashboardNotesCommandHandler : IRequestHandler<UpdateDashboardNotesCommand, ShopDashboardDTO>
    {
        private readonly IShopDashboardRepository _dashboardRepository;

        public UpdateDashboardNotesCommandHandler(IShopDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<ShopDashboardDTO> Handle(UpdateDashboardNotesCommand request, CancellationToken cancellationToken)
        {
            var dashboard = await _dashboardRepository.GetByIdAsync(request.DashboardId.ToString());
            if (dashboard == null)
            {
                throw new ArgumentException($"Dashboard with ID {request.DashboardId} not found");
            }

            dashboard.SetNotes(request.Notes);

            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                dashboard.SetModifier(request.UpdatedBy);
            }

            await _dashboardRepository.ReplaceAsync(dashboard.Id.ToString(), dashboard);

            // Map to DTO
            return new ShopDashboardDTO
            {
                Id = dashboard.Id,
                ShopId = dashboard.ShopId,
                FromTime = dashboard.FromTime,
                ToTime = dashboard.ToTime,
                PeriodType = dashboard.PeriodType,
                TotalLivestream = dashboard.TotalLivestream,
                TotalLivestreamDuration = dashboard.TotalLivestreamDuration,
                TotalLivestreamViewers = dashboard.TotalLivestreamViewers,
                TotalRevenue = dashboard.TotalRevenue,
                OrderInLivestream = dashboard.OrderInLivestream,
                TotalOrder = dashboard.TotalOrder,
                CompleteOrderCount = dashboard.CompleteOrderCount,
                RefundOrderCount = dashboard.RefundOrderCount,
                ProcessingOrderCount = dashboard.ProcessingOrderCount,
                CanceledOrderCount = dashboard.CanceledOrderCount,
                TopOrderProducts = dashboard.TopOrderProducts.Select(p => new TopProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductImageUrl = p.ProductImageUrl,
                    SalesCount = p.SalesCount,
                    Revenue = p.Revenue
                }).ToList(),
                TopAIRecommendedProducts = dashboard.TopAIRecommendedProducts.Select(p => new TopProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductImageUrl = p.ProductImageUrl,
                    SalesCount = p.SalesCount,
                    Revenue = p.Revenue
                }).ToList(),
                RepeatCustomerCount = dashboard.RepeatCustomerCount,
                NewCustomerCount = dashboard.NewCustomerCount,
                Notes = dashboard.Notes,
                CreatedAt = dashboard.CreatedAt,
                LastModifiedAt = dashboard.LastModifiedAt
            };
        }
    }
}