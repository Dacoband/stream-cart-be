using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Shared.Messaging.Extensions
{
    public static class MessagingExtensions
    {
        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IBusRegistrationConfigurator>? configureBus = null)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter(); // Tên queue dạng kebab-case

                // Cho phép caller đăng ký consumers
                configureBus?.Invoke(x);

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = configuration["RabbitMQ:Host"] ?? configuration["RABBITMQ_HOST"];
                    var rabbitMqUsername = configuration["RabbitMQ:Username"] ?? configuration["RABBITMQ_USERNAME"];
                    var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? configuration["RABBITMQ_PASSWORD"];

                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username(rabbitMqUsername);
                        h.Password(rabbitMqPassword);
                    });

                    // Retry & Circuit Breaker (tuỳ chọn)
                    cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(1)));
                    cfg.UseCircuitBreaker(c =>
                    {
                        c.TrackingPeriod = TimeSpan.FromMinutes(1);
                        c.TripThreshold = 1;
                        c.ActiveThreshold = 10;
                    });

                    // Tự động tạo endpoint cho các consumer đã AddConsumer trước đó
                    cfg.ConfigureEndpoints(context);
                });
            });

            // Đăng ký background service cho MassTransit
            services.AddMassTransitHostedService();

            return services;
        }

        // Optional: tiện ích để đăng ký tất cả consumer trong namespace/class nhất định
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
