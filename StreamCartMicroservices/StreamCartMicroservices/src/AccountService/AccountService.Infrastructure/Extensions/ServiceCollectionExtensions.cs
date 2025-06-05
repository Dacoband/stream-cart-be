using AccountService.Infrastructure.Data;
using AccountService.Infrastructure.Interfaces;
using AccountService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Extensions;

namespace AccountService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<AccountContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("PostgreSQL"),
                    npgsqlOptions => npgsqlOptions.MigrationsAssembly(
                        typeof(AccountContext).Assembly.FullName));
            });

            // Register generic repositories from shared library
            services.AddGenericRepositories<AccountContext>();

            // Register service-specific repositories
            services.AddScoped<IAccountRepository, AccountRepository>();
            return services;
        }
    }
}