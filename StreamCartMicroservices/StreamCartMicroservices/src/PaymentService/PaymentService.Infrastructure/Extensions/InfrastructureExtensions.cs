using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Messaging.Publishers;
using PaymentService.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext with PostgreSQL
            services.AddDbContext<PaymentContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("PostgreSQL"),
                    npgsqlOptions => {
                        npgsqlOptions.MigrationsAssembly(typeof(PaymentContext).Assembly.FullName);
                    });
            });
            services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:OrderService"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }

            });

            services.AddHttpClient<IAccountServiceClient, AccountServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:AccountService"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            });
            // Register notification services if needed
            services.AddScoped<IPaymentNotificationService, PaymentNotificationService>();
            // Register message publisher
            services.AddScoped<IMessagePublisher, MessagePublisher>();
            services.AddScoped<IPaymentService, PaymentService.Infrastructure.Services.PaymentService>();
            services.AddScoped<IPaymentRepository, PaymentService.Infrastructure.Repositories.PaymentRepository>();
            services.AddScoped<IQrCodeService, QrCodeService>();


            return services;
        }
    }
}
