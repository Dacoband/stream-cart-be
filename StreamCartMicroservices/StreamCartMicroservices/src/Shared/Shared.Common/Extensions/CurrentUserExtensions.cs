using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Services;
using Shared.Common.Services.User;

namespace Shared.Common.Extensions
{
    public static class CurrentUserExtensions
    {
        public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
        {
           
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
    }
}