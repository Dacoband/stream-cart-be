using CartService.Infrastructure.Data;
using CartService.Infrastructure.Interfaces;
using CartService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shared.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        
            public static IServiceCollection AddInfrastructureServices(
                this IServiceCollection services,
                IConfiguration configuration)
            {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            NpgsqlConnection.GlobalTypeMapper
                .UseJsonNet();
            services.AddDbContext<CartContext>(options =>
                {
                    options.UseNpgsql(
                        configuration.GetConnectionString("PostgreSQL"),
                        npgsqlOptions => {
                            npgsqlOptions.MigrationsAssembly(typeof(CartContext).Assembly.FullName);
                        });
                });
            services.AddGenericRepositories<CartContext>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();

                return services;
            }
        }

    
}
