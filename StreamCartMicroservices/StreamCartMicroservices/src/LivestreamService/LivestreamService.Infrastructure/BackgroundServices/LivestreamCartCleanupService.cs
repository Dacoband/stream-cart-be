using LivestreamService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LivestreamService.Infrastructure.BackgroundServices
{
    public class LivestreamCartCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LivestreamCartCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);
        public LivestreamCartCleanupService(
            IServiceProvider serviceProvider,
            ILogger<LivestreamCartCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🧹 Livestream Cart Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredCartsAsync();
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error occurred during cart cleanup");
                    // Wait a shorter time before retrying on error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("🧹 Livestream Cart Cleanup Service stopped");
        }

        private async Task CleanupExpiredCartsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var cartRepository = scope.ServiceProvider.GetRequiredService<ILivestreamCartRepository>();

            try
            {
                _logger.LogInformation("🧹 Starting cart cleanup process");

                var cleanedCount = await cartRepository.CleanupExpiredCartsAsync();

                if (cleanedCount > 0)
                {
                    _logger.LogInformation("✅ Cart cleanup completed: {CleanedCount} expired carts deactivated", cleanedCount);
                }
                else
                {
                    _logger.LogDebug("🧹 Cart cleanup completed: No expired carts found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during cart cleanup process");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🧹 Livestream Cart Cleanup Service is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}