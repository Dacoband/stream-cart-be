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
        public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter(); // Định dạng tên queue (kebab-case)

                x.AddConsumersFromNamespaceContaining<AccountRegisteredConsumer>(); // Tùy chọn, thêm consumer sau

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = configuration["RabbitMQ:Host"];
                    var rabbitMqUsername = configuration["RabbitMQ:Username"];
                    var rabbitMqPassword = configuration["RabbitMQ:Password"];

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
    }
}
