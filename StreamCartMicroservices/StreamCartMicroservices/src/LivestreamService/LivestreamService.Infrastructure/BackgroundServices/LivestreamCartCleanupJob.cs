using LivestreamService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace LivestreamService.Infrastructure.BackgroundServices
{
    public class LivestreamCartCleanupJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LivestreamCartCleanupJob> _logger;

        public LivestreamCartCleanupJob(
            IServiceProvider serviceProvider,
            ILogger<LivestreamCartCleanupJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            var cartRepository = scope.ServiceProvider.GetRequiredService<ILivestreamCartRepository>();
            var cartItemRepository = scope.ServiceProvider.GetRequiredService<ILivestreamCartItemRepository>();

            try
            {
                _logger.LogInformation("🧹 [Quartz] Starting livestream cart cleanup job");

                var startTime = DateTime.UtcNow;
                var cleanedCount = await cartRepository.CleanupExpiredCartsAsync();

                // Additional cleanup: Remove items from carts that have been inactive for too long
                var inactiveCutoff = DateTime.UtcNow.AddDays(-7); // Remove items older than 7 days
                var expiredItems = await cartItemRepository.FilterByAsync(item =>
                    item.CreatedAt < inactiveCutoff);

                int removedItemsCount = 0;
                foreach (var item in expiredItems)
                {
                    await cartItemRepository.DeleteCartItemAsync(item.Id);
                    removedItemsCount++;
                }

                var duration = DateTime.UtcNow - startTime;

                _logger.LogInformation("✅ [Quartz] Cart cleanup completed in {Duration}ms: {CartCount} carts deactivated, {ItemCount} old items removed",
                    duration.TotalMilliseconds, cleanedCount, removedItemsCount);

                // Set job data for monitoring
                context.Result = new
                {
                    Success = true,
                    CleanedCarts = cleanedCount,
                    RemovedItems = removedItemsCount,
                    Duration = duration.TotalMilliseconds,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [Quartz] Error during cart cleanup job");

                context.Result = new
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };

                throw;
            }
        }
    }
}