using MediatR;
using ShopService.Application.Commands.Dashboard;
using ShopService.Application.DTOs.Dashboard;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries.Dashboard;
using ShopService.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Dashboard
{
    public class GetShopDashboardQueryHandler : IRequestHandler<GetShopDashboardQuery, ShopDashboardDTO>
    {
        private readonly IShopDashboardRepository _dashboardRepository;
        private readonly IMediator _mediator;

        public GetShopDashboardQueryHandler(
            IShopDashboardRepository dashboardRepository,
            IMediator mediator)
        {
            _dashboardRepository = dashboardRepository;
            _mediator = mediator;
        }

        public async Task<ShopDashboardDTO> Handle(GetShopDashboardQuery request, CancellationToken cancellationToken)
        {
            DateTime fromDate = request.FromDate ?? DateTime.UtcNow.Date.AddDays(-30);
            DateTime toDate = request.ToDate ?? DateTime.UtcNow;

            // Check if dashboard exists for the requested period
            var dashboard = await _dashboardRepository.GetDashboardByPeriodAsync(
                request.ShopId, fromDate, toDate, request.PeriodType);

            if (dashboard == null)
            {
                // Generate dashboard if it doesn't exist
                return await _mediator.Send(new GenerateDashboardCommand
                {
                    ShopId = request.ShopId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    PeriodType = request.PeriodType
                }, cancellationToken);
            }

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