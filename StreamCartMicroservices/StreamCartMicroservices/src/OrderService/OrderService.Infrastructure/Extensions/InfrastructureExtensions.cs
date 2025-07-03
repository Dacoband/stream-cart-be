using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Infrastructure.Clients;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Messaging.Consumers;
using OrderService.Infrastructure.Messaging.Publishers;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.Services;
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
            services.AddDbContext<OrderContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("PostgreSQL"),
                    npgsqlOptions => {
                        npgsqlOptions.MigrationsAssembly(typeof(OrderContext).Assembly.FullName);
                    });
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

            

            return services;
        }
    }
}
