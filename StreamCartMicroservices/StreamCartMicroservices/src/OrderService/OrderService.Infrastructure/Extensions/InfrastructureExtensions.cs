using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.NameTranslation;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.BackgroundServices;
using OrderService.Infrastructure.Clients;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Messaging.Consumers;
using OrderService.Infrastructure.Messaging.Publishers;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.Services;
using Quartz;
using Shared.Messaging.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<OrderStatus>("order_status", nameTranslator: new NpgsqlNullNameTranslator());
            NpgsqlConnection.GlobalTypeMapper.MapEnum<PaymentStatus>("payment_status", nameTranslator: new NpgsqlNullNameTranslator());

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("PostgreSQL"));
            dataSourceBuilder.MapEnum<OrderStatus>("order_status", nameTranslator: new NpgsqlNullNameTranslator());
            dataSourceBuilder.MapEnum<PaymentStatus>("payment_status", nameTranslator: new NpgsqlNullNameTranslator());
            var dataSource = dataSourceBuilder.Build();
            services.AddSingleton(dataSource);
            services.AddDbContext<OrderContext>((serviceProvider, options) =>
            {
                var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();
                options.UseNpgsql(
                    dataSource,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(OrderContext).Assembly.FullName);
                        // Add explicit mappings here too
                        npgsqlOptions.MapEnum<OrderStatus>("order_status");
                        npgsqlOptions.MapEnum<PaymentStatus>("payment_status");
                    });

                // Enable unmapped types support
                NpgsqlConnection.GlobalTypeMapper.EnableUnmappedTypes();
            });


            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderService, OrderManagementService >();

            services.AddScoped<IMessagePublisher, MessagePublisher>();

            services.AddHttpClient<IAccountServiceClient, AccountServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:AccountService"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            });

            services.AddHttpClient<IShopServiceClient, ShopServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:ShopService"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            });
            services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:ProductService"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            });
            services.AddHttpClient<IWalletServiceClient, WalletServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:WalletService"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            });
            services.AddHostedService<OrderCompletionService>();
            services.AddQuartz(q =>
            {
                var jobKey = new JobKey("AutoOrderCompleteJob");
                q.AddJob<AutoOrderCompleteJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("AutoOrderCompleteJob-trigger")
                    .WithCronSchedule("0 0 * * * ?")); // Chạy mỗi giờ
            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            services.AddScoped<IAdressServiceClient, AddressServiceClient>();
            services.AddScoped<IMembershipServiceClient, MembershipServiceClient>();
            services.AddScoped<IShopVoucherClientService,ShopVoucherServiceClient>();
            services.AddSingleton<IOrderNotificationQueue, OrderNotificationQueue>();
            services.AddHostedService<OrderNotificationWorker>();
            return services;
        }
    }
}
