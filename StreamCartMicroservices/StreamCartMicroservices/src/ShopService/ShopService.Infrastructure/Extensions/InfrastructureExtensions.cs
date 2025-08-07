using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Extensions;
using ShopService.Application.Interfaces;
using ShopService.Application.Services;
using ShopService.Infrastructure.Data;
using ShopService.Infrastructure.Messaging.Consumers;
using ShopService.Infrastructure.Messaging.Publishers;
using ShopService.Infrastructure.Repositories;

namespace ShopService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext with PostgreSQL
            services.AddDbContext<ShopContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("PostgreSQL"),
                    npgsqlOptions => {
                        npgsqlOptions.MigrationsAssembly(typeof(ShopContext).Assembly.FullName);
                    });
            });

            // Register repository implementations
            services.AddScoped<IShopRepository, ShopRepository>();
            
            // Register message publisher
            services.AddScoped<IMessagePublisher, MessagePublisher>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IShopVoucherRepository, ShopVoucherRepository>();


            services.AddScoped<IShopDashboardService, ShopDashboardService>();

            services.AddScoped<IMembershipRepository, MembershipRepository>();
            services.AddScoped<IShopMembershipRepository, ShopMembershipRepository>();
            services.AddScoped<IShopDashboardRepository, ShopDashboardRepository>();
            services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
            services.AddScoped<IShopUnitOfWork, ShopUnitOfWork>();
            return services;
        }
    }
}
