using LivestreamService.Application.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LivestreamService.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register MediatR handlers
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(typeof(CreateLivestreamCommand).Assembly);
            });

            return services;
        }
    }
}