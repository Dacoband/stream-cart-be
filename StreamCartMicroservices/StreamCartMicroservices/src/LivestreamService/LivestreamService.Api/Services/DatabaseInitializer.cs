using LivestreamService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LivestreamService.Api.Services
{
    public class DatabaseInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(
            IServiceProvider serviceProvider,
            ILogger<DatabaseInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing database...");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LivestreamDbContext>();

            try
            {
                // Apply migrations
                await dbContext.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Database migrations applied successfully");

                // Seed sample data in development environment
                if (!dbContext.Livestreams.Any())
                {
                    await SeedDataAsync(dbContext, cancellationToken);
                    _logger.LogInformation("Sample data seeded successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
        }
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task SeedDataAsync(LivestreamDbContext dbContext, CancellationToken cancellationToken)
        {
            // Add seed data for development/testing if needed
            // Example:
            /*
            var seller1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var shop1Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
            
            var livestream1 = new Livestream(
                "First Test Livestream",
                "This is a test livestream for development",
                seller1Id,
                shop1Id,
                DateTime.UtcNow.AddDays(1),
                "test-room-id-1",
                "test-stream-key-1",
                "https://example.com/thumbnails/test1.jpg",
                "test,development",
                "system"
            );
            
            dbContext.Livestreams.Add(livestream1);
            await dbContext.SaveChangesAsync(cancellationToken);
            */
        }
    }
}