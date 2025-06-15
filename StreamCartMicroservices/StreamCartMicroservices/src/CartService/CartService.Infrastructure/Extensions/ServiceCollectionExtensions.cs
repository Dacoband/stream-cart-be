using CartService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                services.AddDbContext<CartContext>(options =>
                {
                    options.UseNpgsql(
                        configuration.GetConnectionString("PostgreSQL"),
                        npgsqlOptions => {
                            npgsqlOptions.MigrationsAssembly(typeof(CartContext).Assembly.FullName);
                        });
                });


                return services;
            }
        }

    
}
