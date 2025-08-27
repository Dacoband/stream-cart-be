using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Application.Handlers;
using AccountService.Application.Interfaces;
using AccountService.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ShopService.Application.Interfaces;
using ShopService.Application.Services;
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
            // Register MediatR handlers explicitly if needed
             services.AddScoped<IRequestHandler<CreateAccountCommand, AccountDto>, CreateAccountCommandHandler>();
             services.AddScoped<IRequestHandler<UpdateAccountCommand, AccountDto>, UpdateAccountCommandHandler>();
           // services.AddScoped<IRequestHandler<UpdateLastLoginCommand, AccountDto>, UpdateLastLoginCommandHandler>();


            // Register services
            services.AddScoped<IAccountManagementService, AccountManagementService>();
            services.AddScoped<IAddressManagementService, AddressManagementService>();
            services.AddScoped<IAuthService, AuthService>();
            
            services.AddHttpClient<IShopServiceClient, ShopServiceClient>();
            return services;
        }
    }
}
