using DeliveryService.Application.Interfaces;
using DeliveryService.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryService.Application.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            //Register MediatR
            //services.AddMediatR(config =>
            //{
            //    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            //});

            // Register services
            services.AddScoped<IDeliveryAddressInterface, DeliveryAddressService>();
            services.AddScoped<IAddressClientService, AddressClientService>();
            

            return services;
        }
    }

}
