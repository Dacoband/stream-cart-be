using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Infrastructure.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Api.Services
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
            _logger.LogInformation("Ensuring database is created and migrated...");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderContext>();

            await dbContext.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("Database check completed.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}