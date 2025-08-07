using MediatR;
using ShopService.Application.Commands.Dashboard;
using ShopService.Application.DTOs.Dashboard;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Dashboard
{
    public class GenerateDashboardCommandHandler : IRequestHandler<GenerateDashboardCommand, ShopDashboardDTO>
    {
        private readonly IShopDashboardRepository _dashboardRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IOrderServiceClient _orderServiceClient;
        private readonly ILivestreamServiceClient _livestreamServiceClient;
        private readonly IProductServiceClient _productServiceClient;

        public GenerateDashboardCommandHandler(
            IShopDashboardRepository dashboardRepository,
            IShopRepository shopRepository,
            IOrderServiceClient orderServiceClient,
            ILivestreamServiceClient livestreamServiceClient,
            IProductServiceClient productServiceClient)
        {
            _dashboardRepository = dashboardRepository;
            _shopRepository = shopRepository;
            _orderServiceClient = orderServiceClient;
            _livestreamServiceClient = livestreamServiceClient;
            _productServiceClient = productServiceClient;
        }

        public async Task<ShopDashboardDTO> Handle(GenerateDashboardCommand request, CancellationToken cancellationToken)
        {
            // Verify shop exists
            var shop = await _shopRepository.GetByIdAsync(request.ShopId.ToString());
            if (shop == null)
            {
                throw new ArgumentException($"Shop with ID {request.ShopId} not found");
            }

            // Check if dashboard for this period already exists
            var existingDashboard = await _dashboardRepository.GetDashboardByPeriodAsync(
                request.ShopId, request.FromDate, request.ToDate, request.PeriodType);

            ShopDashboard dashboard;

            if (existingDashboard != null)
            {
                dashboard = existingDashboard;
            }
            else
            {
                // Create new dashboard
                dashboard = new ShopDashboard(
                    request.ShopId,
                    request.FromDate,
                    request.ToDate,
                    request.PeriodType);

                if (!string.IsNullOrEmpty(request.GeneratedBy))
                {
                    dashboard.SetCreator(request.GeneratedBy);
                }
            }

            // Gather data from various services
            var orderStats = await _orderServiceClient.GetOrderStatisticsAsync(
                request.ShopId, request.FromDate, request.ToDate);

            var livestreamStats = await _livestreamServiceClient.GetLivestreamStatisticsAsync(
                request.ShopId, request.FromDate, request.ToDate);

            var topProducts = await _orderServiceClient.GetTopSellingProductsAsync(
                request.ShopId, request.FromDate, request.ToDate);

            var topAIProducts = await _productServiceClient.GetTopAIRecommendedProductsAsync(
                request.ShopId, request.FromDate, request.ToDate);

            var customerStats = await _orderServiceClient.GetCustomerStatisticsAsync(
                request.ShopId, request.FromDate, request.ToDate);

            // Update dashboard with collected data
            dashboard.UpdateOrderStatistics(
                orderStats.TotalRevenue,
                orderStats.TotalOrders,
                orderStats.OrdersInLivestream,
                orderStats.CompleteOrderCount,
                orderStats.RefundOrderCount,
                orderStats.ProcessingOrderCount,
                orderStats.CanceledOrderCount);

            dashboard.UpdateLivestreamStatistics(
                livestreamStats.TotalLivestreams,
                livestreamStats.TotalDuration,
                livestreamStats.TotalViewers);

            dashboard.UpdateCustomerStatistics(
                customerStats.RepeatCustomers,
                customerStats.NewCustomers);

            // Map top products
            var topProductsList = topProducts.Products.Select(p => new TopProductInfo
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductImageUrl = p.ProductImageUrl,
                SalesCount = p.SalesCount,
                Revenue = p.Revenue
            }).ToList();

            dashboard.SetTopOrderProducts(topProductsList);

            var topAIProductsList = topAIProducts.Products.Select(p => new TopProductInfo
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductImageUrl = p.ProductImageUrl,
                SalesCount = p.SalesCount,
                Revenue = p.Revenue
            }).ToList();

            dashboard.SetTopAIProducts(topAIProductsList);

            // Save dashboard
            if (existingDashboard == null)
            {
                await _dashboardRepository.InsertAsync(dashboard);
            }
            else
            {
                await _dashboardRepository.ReplaceAsync(dashboard.Id.ToString(), dashboard);
            }

            // Return DTO
            return MapToDTO(dashboard);
        }

        private ShopDashboardDTO MapToDTO(ShopDashboard dashboard)
        {
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