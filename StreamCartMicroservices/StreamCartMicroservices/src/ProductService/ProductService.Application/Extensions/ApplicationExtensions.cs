using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ProductService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Đăng ký MediatR handlers
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            return services;
        }
    }
}