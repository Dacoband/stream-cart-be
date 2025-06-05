using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messaging.Extensions
{
    public static class MessagingExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration, Action<IBusRegistrationConfigurator>? configureBus = null)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter(); // Định dạng tên queue (kebab-case)

                // Allow caller to configure consumers or other settings
                configureBus?.Invoke(x);

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = configuration["RabbitMQSettings:Host"];
                    var rabbitMqUsername = configuration["RabbitMQSettings:Username"];
                    var rabbitMqPassword = configuration["RabbitMQSettings:Password"];

                    // Check if the host is null and provide proper error handling
                    if (string.IsNullOrEmpty(rabbitMqHost))
                    {
                        throw new InvalidOperationException("RabbitMQ host configuration is missing. Check 'RabbitMQSettings:Host' in configuration.");
                    }

                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username(rabbitMqUsername);
                        h.Password(rabbitMqPassword);
                    });

                    // Cấu hình retry policy (tùy chọn)
                    cfg.UseMessageRetry(r => r.Interval(5, 1000)); // Thử lại 5 lần, mỗi lần cách 1 giây

                    // Cấu hình circuit breaker (tùy chọn)
                    cfg.UseCircuitBreaker(c =>
                    {
                        c.TrackingPeriod = TimeSpan.FromMinutes(1);
                        c.TripThreshold = (int)0.5; // Ngắt khi 50% lỗi
                        c.ActiveThreshold = 10; // Ít nhất 10 thông điệp
                    });
                });
            });

            // Đăng ký IBus và IPublishEndpoint
            services.AddMassTransitHostedService();

            return services;
        }

        // Modified extension method that doesn't call AddMassTransit again
        public static IServiceCollection AddConsumers<T>(this IServiceCollection services)
            where T : class
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumersFromNamespaceContaining<T>();
            });

            return services;
        }
    }
}