using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Clients;
using OrderService.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service that processes completed orders and updates shop statistics
    /// </summary>
    public class OrderCompletionService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderCompletionService> _logger;
        private readonly TimeSpan _processInterval = TimeSpan.FromHours(6); // Run every 6 hours

        public OrderCompletionService(
            IServiceScopeFactory scopeFactory,
            ILogger<OrderCompletionService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order Completion Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessCompletedOrdersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing completed orders");
                }

                await Task.Delay(_processInterval, stoppingToken);
            }

            _logger.LogInformation("Order Completion Service stopped");
        }

        private async Task ProcessCompletedOrdersAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting to process completed orders");

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderContext>();
            var shopClient = scope.ServiceProvider.GetRequiredService<IShopServiceClient>();

            try
            {
                // ✅ FIX: Sử dụng Raw SQL để tránh enum conversion issue
                var completedOrdersQuery = @"
                    SELECT shop_id as ""ShopId"", COUNT(*) as ""OrderCount""
                    FROM orders 
                    WHERE order_status = 'Delivered'::order_status 
                      AND NOT is_deleted 
                    GROUP BY shop_id";

                var completedOrders = await dbContext.Database
                    .SqlQueryRaw<ShopOrderCount>(completedOrdersQuery)
                    .ToListAsync(stoppingToken);

                _logger.LogInformation("Found {Count} shops with completed orders to process", completedOrders.Count);

                // Process each shop's completion rate
                foreach (var shopData in completedOrders)
                {
                    try
                    {
                        // The actual formula could be adjusted based on business rules
                        // Here we use a small positive adjustment for each batch of completed orders
                        decimal completionRateAdjustment = 0.5m; // 0.5% increase per batch of completed orders

                        // Update the shop's completion rate via the ShopService
                        var result = await shopClient.UpdateShopCompletionRateAsync(
                            shopData.ShopId,
                            completionRateAdjustment,
                            Guid.Parse("00000000-0000-0000-0000-000000000001")); // System user ID

                        if (result)
                        {
                            _logger.LogInformation(
                                "Updated completion rate for shop {ShopId} by +{Rate}%",
                                shopData.ShopId,
                                completionRateAdjustment);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Failed to update completion rate for shop {ShopId}",
                                shopData.ShopId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error updating completion rate for shop {ShopId}",
                            shopData.ShopId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing completed orders query");
                throw;
            }

            _logger.LogInformation("Finished processing completed orders");
        }

        // ✅ Helper class để mapping kết quả SQL query
        public class ShopOrderCount
        {
            public Guid ShopId { get; set; }
            public int OrderCount { get; set; }
        }
    }
}