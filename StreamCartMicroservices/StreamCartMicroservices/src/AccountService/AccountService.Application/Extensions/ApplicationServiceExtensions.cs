using AccountService.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace AccountService.Application.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            // Register application services
            services.AddScoped<AccountManagementService>();

            return services;
        }
    }
}
