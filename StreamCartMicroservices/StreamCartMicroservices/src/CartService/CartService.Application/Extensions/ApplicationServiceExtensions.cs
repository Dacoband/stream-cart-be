using CartService.Application.Interfaces;
using CartService.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register MediatR
            //services.AddMediatR(config =>
            //{
            //    config.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly);
            //});

            // Register services
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICartService, CartService.Application.Services.CartService>();

            return services;
        }
    }
}
