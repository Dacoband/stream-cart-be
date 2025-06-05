using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Settings;

namespace Shared.Common.Extensions
{
    public static class SettingsExtensions
    {
        public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AppSettings>(configuration);
            services.Configure<ConnectionStrings>(configuration.GetSection(nameof(AppSettings.ConnectionStrings)));
            services.Configure<JwtSettings>(configuration.GetSection(nameof(AppSettings.JwtSettings)));
            services.Configure<CorsSettings>(configuration.GetSection(nameof(AppSettings.CorsSettings)));
            
            return services;
        }
    }
}