using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Interfaces;
using ProductService.Infrastructure.Repositories;
using Shared.Common.Extensions;

namespace ProductService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext with PostgreSQL
            services.AddDbContext<ProductContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("PostgreSQL"),
                    npgsqlOptions => {
                        npgsqlOptions.MigrationsAssembly(typeof(ProductContext).Assembly.FullName);
                    });
            });

            // Register generic repositories
            services.AddGenericRepositories<ProductContext>();

            // Register specific repositories
            services.AddScoped<IProductRepository, ProductRepository>();

            return services;
        }
    }
}