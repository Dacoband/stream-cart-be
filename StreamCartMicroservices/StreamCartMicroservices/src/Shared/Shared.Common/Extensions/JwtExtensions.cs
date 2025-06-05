using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Common.Settings;
using System.Text;

namespace Shared.Common.Extensions
{
    public static class JwtExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection($"{nameof(AppSettings.JwtSettings)}");
            var jwtSettings = new JwtSettings
            {
                SecretKey = jwtSection.GetValue<string>("SecretKey") ?? string.Empty,
                Issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty,
                Audience = jwtSection.GetValue<string>("Audience") ?? string.Empty,
                ExpiryMinutes = int.TryParse(jwtSection.GetValue<string>("ExpiryMinutes"), out int minutes) ? minutes : 60
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                };
            });

            return services;
        }
    }
}