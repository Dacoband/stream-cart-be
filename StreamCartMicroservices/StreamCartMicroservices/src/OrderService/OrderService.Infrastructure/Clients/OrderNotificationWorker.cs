using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using Shared.Messaging.Event.OrderEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Clients
{
    public class OrderNotificationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOrderNotificationQueue _notificationQueue;
        private readonly ILogger<OrderNotificationWorker> _logger;

        public OrderNotificationWorker(
            IServiceProvider serviceProvider,
            IOrderNotificationQueue notificationQueue,
            ILogger<OrderNotificationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _notificationQueue = notificationQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var notification = await _notificationQueue.DequeueAsync(stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                try
                {
                    await publishEndpoint.Publish(notification, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish order notification.");
                }
            }
        }
    }

}
