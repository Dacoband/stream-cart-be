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
            // Mapping enum PostgreSQL
            //NpgsqlConnection.GlobalTypeMapper.MapEnum<OrderStatus>("order_status", nameTranslator: new NpgsqlNullNameTranslator());
            //NpgsqlConnection.GlobalTypeMapper.MapEnum<PaymentStatus>("payment_status", nameTranslator: new NpgsqlNullNameTranslator());

            // Cấu hình DataSource
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("PostgreSQL"));
            //dataSourceBuilder.EnableUnmappedTypes();
            dataSourceBuilder.MapEnum<OrderStatus>("order_status", nameTranslator: new NpgsqlNullNameTranslator());
            dataSourceBuilder.MapEnum<PaymentStatus>("payment_status", nameTranslator: new NpgsqlNullNameTranslator());
            var dataSource = dataSourceBuilder.Build();
            services.AddSingleton(dataSource);
            // Cấu hình DbContext
            services.AddDbContext<OrderContext>((serviceProvider, options) =>
            {
                var ds = serviceProvider.GetRequiredService<NpgsqlDataSource>();
                options.UseNpgsql(ds, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(OrderContext).Assembly.FullName);

                });
            });

            // Đăng ký Repository & Services
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderService, OrderManagementService>();
            services.AddScoped<IOrderItemService, OrderItemManagementService>();
            services.AddScoped<IReviewRepository,ReviewRepository>();
            services.AddSingleton<IOrderNotificationQueue, OrderNotificationQueue>();
            services.AddHostedService<OrderNotificationWorker>();

            // Messaging
            services.AddScoped<IMessagePublisher, MessagePublisher>();

            // HTTP Clients
            services.AddHttpClient<IAccountServiceClient, AccountServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:AccountService"];
                if (!string.IsNullOrEmpty(baseUrl))
                    client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient<IShopServiceClient, ShopServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:ShopService"];
                if (!string.IsNullOrEmpty(baseUrl))
                    client.BaseAddress = new Uri(baseUrl);
            });
            services.AddHttpClient<ILivestreamServiceClient, LivestreamServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:LivestreamService"];
                if (!string.IsNullOrEmpty(baseUrl))
                    client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:ProductService"];
                if (!string.IsNullOrEmpty(baseUrl))
                    client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient<IWalletServiceClient, WalletServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:WalletService"];
                if (!string.IsNullOrEmpty(baseUrl))
                    client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient<IAdressServiceClient, AddressServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:AddressService"];
                if (!string.IsNullOrEmpty(baseUrl))
                    client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient<IMembershipServiceClient, MembershipServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:MembershipService"];
                if (!string.IsNullOrEmpty(baseUrl))
                    client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient<IShopVoucherClientService, ShopVoucherServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:VoucherService"];
                if (!string.IsNullOrEmpty(baseUrl))
                    client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient<IDeliveryClient, DeliveryServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:DeliveryService"];
                if (!string.IsNullOrEmpty(baseUrl))
                    client.BaseAddress = new Uri(baseUrl);
            });
            services.AddHttpClient<IDeliveryApiClient, DeliveryApiClient>();

            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                // Order tracking status update job - runs every 10 minutes
                var trackingJobKey = new JobKey("OrderTrackingStatusUpdateJob");
                q.AddJob<OrderTrackingStatusUpdateJob>(opts => opts.WithIdentity(trackingJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(trackingJobKey)
                    .WithIdentity("OrderTrackingStatusUpdateTrigger")
                    .WithCronSchedule("0 */2 * * * ?")
                    .WithDescription("Update order status based on delivery tracking"));

                var cancelDraftingKey = new JobKey("cancel-drafting-orders-job");
                q.AddJob<CancelDraftingOrdersJob>(opts => opts.WithIdentity(cancelDraftingKey));
                q.AddTrigger(opts => opts
                    .ForJob(cancelDraftingKey)
                    .WithIdentity("cancel-drafting-orders-trigger")
                    .WithCronSchedule("0 */5 * * * ?"));

                var cancelPendingKey = new JobKey("cancel-pending-orders-job");
                q.AddJob<CancelPendingOrdersJob>(opts => opts.WithIdentity(cancelPendingKey));
                q.AddTrigger(opts => opts
                    .ForJob(cancelPendingKey)
                    .WithIdentity("cancel-pending-orders-trigger")
                    .WithCronSchedule("0 0 */1 * * ?"));

                var cancelProcessingKey = new JobKey("cancel-processing-orders-job");
                q.AddJob<CancelProcessingOrdersJob>(opts => opts.WithIdentity(cancelProcessingKey));
                q.AddTrigger(opts => opts
                    .ForJob(cancelProcessingKey)
                    .WithIdentity("cancel-processing-orders-trigger")
                    .WithCronSchedule("0 30 */1 * * ?"));

                var autoCompleteKey = new JobKey("auto-complete-delivered-orders-job");
                q.AddJob<AutoCompleteDeliveredOrdersJob>(opts => opts.WithIdentity(autoCompleteKey));
                q.AddTrigger(opts => opts
                    .ForJob(autoCompleteKey)
                    .WithIdentity("auto-complete-delivered-orders-trigger")
                    .WithCronSchedule("0 30 0 * * ?"));
            });
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            // Nếu có service chạy nền theo interval ngoài Quartz
            services.AddHostedService<OrderCompletionService>();
            services.AddHttpClient<ILivestreamServiceClient, LivestreamServiceClient>();

            return services;
        }
    }
}
