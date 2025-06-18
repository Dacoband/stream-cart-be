using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopService.Application.Interfaces;
using ShopService.Infrastructure.Data;
using ShopService.Infrastructure.Messaging.Consumers;
using ShopService.Infrastructure.Messaging.Publishers;
using ShopService.Infrastructure.Repositories;
using Shared.Common.Extensions;
using MassTransit;

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
            
 

            return services;
        }
    }
}
