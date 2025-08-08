using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Settings;

namespace Shared.Common.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddConfiguredCors(
            this IServiceCollection services,
            IConfiguration configuration,
            string policyName = "DefaultCorsPolicy")
        {
            var corsSettings = configuration
                .GetSection($"{nameof(AppSettings.CorsSettings)}")
                .Get<CorsSettings>() ?? new CorsSettings();

            services.AddCors(options =>
            {
                options.AddPolicy(policyName, builder =>
                {
                    if (corsSettings.AllowedOrigins.Length > 0)
                    {
                        if (corsSettings.AllowedOrigins.Contains("*"))
                        {
                           // builder.AllowAnyOrigin();
                            builder.SetIsOriginAllowed(_ => true);
                        }
                        else
                        {
                            builder.WithOrigins(corsSettings.AllowedOrigins);
                        }
                    }

                    if (corsSettings.AllowedMethods.Length > 0)
                    {
                        if (corsSettings.AllowedMethods.Contains("*"))
                        {
                            builder.AllowAnyMethod();
                        }
                        else
                        {
                            builder.WithMethods(corsSettings.AllowedMethods);
                        }
                    }
                    else
                    {
                        builder.AllowAnyMethod();
                    }

                    if (corsSettings.AllowedHeaders.Length > 0)
                    {
                        if (corsSettings.AllowedHeaders.Contains("*"))
                        {
                            builder.AllowAnyHeader();
                        }
                        else
                        {
                            builder.WithHeaders(corsSettings.AllowedHeaders);
                        }
                    }
                    else
                    {
                        builder.AllowAnyHeader();
                    }

                    if (corsSettings.AllowCredentials)
                    {
                        builder.AllowCredentials();
                    }

                    if (corsSettings.MaxAge > 0)
                    {
                        builder.SetPreflightMaxAge(TimeSpan.FromSeconds(corsSettings.MaxAge));
                    }
                    builder.WithExposedHeaders("*");
                });
            });

            return services;
        }

        public static IApplicationBuilder UseConfiguredCors(
            this IApplicationBuilder app, 
            string policyName = "DefaultCorsPolicy")
        {
            return app.UseCors(policyName);
        }
    }
}