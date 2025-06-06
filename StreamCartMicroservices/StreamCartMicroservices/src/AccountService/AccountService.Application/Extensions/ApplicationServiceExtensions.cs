using AccountService.Application.Commands;
using AccountService.Application.Interfaces;
using AccountService.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AccountService.Application.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register MediatR
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly);
            });

            // Register services
            services.AddScoped<IAccountManagementService, AccountManagementService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
