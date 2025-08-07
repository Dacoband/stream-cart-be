using MediatR;
using ShopService.Application.DTOs.Dashboard;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries.Dashboard;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Dashboard
{
    public class GetShopDashboardSummaryQueryHandler : IRequestHandler<GetShopDashboardSummaryQuery, ShopDashboardSummaryDTO>
    {
        private readonly IShopDashboardRepository _dashboardRepository;
        private readonly IOrderServiceClient _orderServiceClient;
        private readonly ILivestreamServiceClient _livestreamServiceClient;
        private readonly IShopRepository _shopRepository;

        public GetShopDashboardSummaryQueryHandler(
            IShopDashboardRepository dashboardRepository,
            IOrderServiceClient orderServiceClient,
            ILivestreamServiceClient livestreamServiceClient,
            IShopRepository shopRepository)
        {
            _dashboardRepository = dashboardRepository;
            _orderServiceClient = orderServiceClient;
            _livestreamServiceClient = livestreamServiceClient;
            _shopRepository = shopRepository;
        }

        public async Task<ShopDashboardSummaryDTO> Handle(GetShopDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            // Verify shop exists
            var shop = await _shopRepository.GetByIdAsync(request.ShopId.ToString());
            if (shop == null)
            {
                throw new ArgumentException($"Shop with ID {request.ShopId} not found");
            }

            // Get the most recent dashboard for this shop
            var dashboard = await _dashboardRepository.GetLatestDashboardAsync(request.ShopId, "daily");

            // If no dashboard exists, get real-time statistics for the last 30 days
            if (dashboard == null)
            {
                var fromDate = DateTime.UtcNow.Date.AddDays(-30);
                var toDate = DateTime.UtcNow;

                var orderStats = await _orderServiceClient.GetOrderStatisticsAsync(
                    request.ShopId, fromDate, toDate);

                var livestreamStats = await _livestreamServiceClient.GetLivestreamStatisticsAsync(
                    request.ShopId, fromDate, toDate);

                return new ShopDashboardSummaryDTO
                {
                    ShopId = request.ShopId,
                    TotalRevenue = orderStats.TotalRevenue,
                    TotalOrders = orderStats.TotalOrders,
                    TotalLivestreams = livestreamStats.TotalLivestreams,
                    TotalCustomers = orderStats.TotalOrders > 0 ? orderStats.TotalOrders / 2 : 0, // Approximate
                    CompletionRate = orderStats.TotalOrders > 0
                        ? (decimal)orderStats.CompleteOrderCount / orderStats.TotalOrders * 100
                        : 0,
                    LastUpdated = DateTime.UtcNow
                };
            }

            // Return summary data from the existing dashboard
            return new ShopDashboardSummaryDTO
            {
                ShopId = dashboard.ShopId,
                TotalRevenue = dashboard.TotalRevenue,
                TotalOrders = dashboard.TotalOrder,
                TotalLivestreams = dashboard.TotalLivestream,
                TotalCustomers = dashboard.RepeatCustomerCount + dashboard.NewCustomerCount,
                CompletionRate = dashboard.TotalOrder > 0
                    ? (decimal)dashboard.CompleteOrderCount / dashboard.TotalOrder * 100
                    : 0,
                LastUpdated = dashboard.LastModifiedAt ?? dashboard.CreatedAt
            };
        }
    }
}