﻿using LivestreamService.Application.Interfaces;
using LivestreamService.Infrastructure.Data;
using LivestreamService.Infrastructure.Repositories;
using LivestreamService.Infrastructure.Services;
using LivestreamService.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LivestreamService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add database
            services.AddDbContext<LivestreamDbContext>(options =>
            {
                options.UseNpgsql(
                   configuration.GetConnectionString("PostgreSQL"),
                   npgsqlOptions => {
                       npgsqlOptions.MigrationsAssembly(typeof(LivestreamDbContext).Assembly.FullName);
                   });
            });
            services.Configure<MongoDBSettings>(options =>
            {
                options.ConnectionString = configuration["MONGO_CONNECTION_STRING"] ??
                    throw new InvalidOperationException("MongoDB connection string not found");
                options.DatabaseName = "LivestreamChatDB";
                options.LivestreamChatCollectionName = "LivestreamChats";
                options.ChatRoomCollectionName = "ChatRooms";
                options.ChatMessageCollectionName = "ChatMessages";
            });

            services.AddSingleton<MongoDbContext>();

            // Add repositories
            services.AddScoped<ILivestreamRepository, LivestreamRepository>();
            services.AddScoped<ILivestreamProductRepository, LivestreamProductRepository>();
            services.AddScoped<IStreamEventRepository, StreamEventRepository>();
            services.AddScoped<IStreamViewRepository, StreamViewRepository>();


            // Add service clients
            services.AddScoped<IShopServiceClient, ShopServiceClient>();
            services.AddScoped<IAccountServiceClient, AccountServiceClient>();
            services.AddScoped<ILivekitService, LivekitService>();

            // Add HttpClient for service communication
            services.AddHttpClient<IShopServiceClient, ShopServiceClient>(client =>
            {
                var serviceUrl = configuration["ServiceUrls:ShopService"];
                if(!string.IsNullOrEmpty(serviceUrl))
                {
                    client.BaseAddress = new Uri(serviceUrl);
                }
            });

            services.AddHttpClient<IAccountServiceClient, AccountServiceClient>(client =>
            {
                var serviceUrl = configuration["ServiceUrls:AccountService"];
                if (!string.IsNullOrEmpty(serviceUrl))
                {
                    client.BaseAddress = new Uri(serviceUrl);
                }
            });
            services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
            {
                var serviceUrl = configuration["ServiceUrls:ProductService"];
                if (!string.IsNullOrEmpty(serviceUrl))
                {
                    client.BaseAddress = new Uri(serviceUrl);
                }
            });
            // Thêm vào các dang ký service
            services.AddHttpClient<IProductServiceClient, ProductServiceClient>();
            // Thêm vào ConfigureRepositories method
            services.AddScoped<ILivestreamChatRepository, LivestreamChatRepository>();
            services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

            // Thêm SignalR
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                options.KeepAliveInterval = TimeSpan.FromSeconds(30);
            });
            services.AddScoped<IChatNotificationServiceSignalR, ChatNotificationServiceSignalR>();
            return services;

        }

        //public static IServiceCollection AddHttpClientFactory(this IServiceCollection services)
        //{
        //    services.AddHttpClient();
        //    return services;
        //}
    }
}