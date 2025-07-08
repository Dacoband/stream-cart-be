using LivestreamService.Application.Interfaces;
using LivestreamService.Infrastructure.Data;
using LivestreamService.Infrastructure.Repositories;
using LivestreamService.Infrastructure.Services;
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

            // Add repositories
            services.AddScoped<ILivestreamRepository, LivestreamRepository>();

            // Add service clients
            services.AddScoped<IShopServiceClient, ShopServiceClient>();
            services.AddScoped<IAccountServiceClient, AccountServiceClient>();
            services.AddScoped<ILivekitService, LivekitService>();

            // Add HttpClient for service communication
            services.AddHttpClient<IShopServiceClient, ShopServiceClient>(client =>
            {
                var serviceUrl = configuration["ServiceUrls:ShopService"] ?? "http://shop-service";
                client.BaseAddress = new Uri(serviceUrl);
            });

            services.AddHttpClient<IAccountServiceClient, AccountServiceClient>(client =>
            {
                var serviceUrl = configuration["ServiceUrls:AccountService"] ?? "http://account-service";
                client.BaseAddress = new Uri(serviceUrl);
            });

            return services;
        }

        public static IServiceCollection AddHttpClientFactory(this IServiceCollection services)
        {
            services.AddHttpClient();
            return services;
        }
    }
}