using AccountService.Application.Hanlders;
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
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAccountHandler).Assembly));

            // Register application services
            services.AddScoped<AccountManagementService>();

            return services;
        }
    }
}
