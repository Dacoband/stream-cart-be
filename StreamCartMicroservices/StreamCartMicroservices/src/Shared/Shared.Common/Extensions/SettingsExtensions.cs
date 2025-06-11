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
            var jwtSettings = new JwtSettings
            {
                SecretKey = GetEnvironmentVariableOrConfig(configuration, "JWT_SECRET_KEY", $"{nameof(AppSettings.JwtSettings)}:{nameof(JwtSettings.SecretKey)}"),
                Issuer = GetEnvironmentVariableOrConfig(configuration, "JWT_ISSUER", $"{nameof(AppSettings.JwtSettings)}:{nameof(JwtSettings.Issuer)}"),
                Audience = GetEnvironmentVariableOrConfig(configuration, "JWT_AUDIENCE", $"{nameof(AppSettings.JwtSettings)}:{nameof(JwtSettings.Audience)}"),
                ExpiryMinutes = int.TryParse(
                    GetEnvironmentVariableOrConfig(configuration, "JWT_EXPIRY_MINUTES", $"{nameof(AppSettings.JwtSettings)}:{nameof(JwtSettings.ExpiryMinutes)}"),
                    out int minutes) ? minutes : 60
            };
            services.Configure<JwtSettings>(options =>
            {
                options.SecretKey = jwtSettings.SecretKey;
                options.Issuer = jwtSettings.Issuer;
                options.Audience = jwtSettings.Audience;
                options.ExpiryMinutes = jwtSettings.ExpiryMinutes;
            });
            services.Configure<CorsSettings>(configuration.GetSection(nameof(AppSettings.CorsSettings)));
            var emailSettings = new EmailSettings
            {
                ApiKey = GetEnvironmentVariableOrConfig(configuration, "EMAIL_API_KEY", $"{nameof(AppSettings.EmailSettings)}:{nameof(EmailSettings.ApiKey)}"),
                SecretKey = GetEnvironmentVariableOrConfig(configuration, "EMAIL_SECRET_KEY", $"{nameof(AppSettings.EmailSettings)}:{nameof(EmailSettings.SecretKey)}"),
                DefaultFromEmail = GetEnvironmentVariableOrConfig(configuration, "EMAIL_FROM_EMAIL", $"{nameof(AppSettings.EmailSettings)}:{nameof(EmailSettings.DefaultFromEmail)}"),
                DefaultFromName = GetEnvironmentVariableOrConfig(configuration, "EMAIL_FROM_NAME", $"{nameof(AppSettings.EmailSettings)}:{nameof(EmailSettings.DefaultFromName)}"),
                Provider = GetEnvironmentVariableOrConfig(configuration, "EMAIL_PROVIDER", $"{nameof(AppSettings.EmailSettings)}:{nameof(EmailSettings.Provider)}")
            };
            services.Configure<EmailSettings>(options =>
            {
                options.ApiKey = emailSettings.ApiKey;
                options.SecretKey = emailSettings.SecretKey;
                options.DefaultFromEmail = emailSettings.DefaultFromEmail;
                options.DefaultFromName = emailSettings.DefaultFromName;
                options.Provider = emailSettings.Provider;
            });

            return services;
        }
        /// <summary>
        /// Lấy giá trị từ biến môi trường hoặc configuration nếu không tìm thấy
        /// </summary>
        private static string GetEnvironmentVariableOrConfig(IConfiguration configuration, string envKey, string configKey)
        {
            // Đầu tiên thử lấy từ biến môi trường
            var envValue = Environment.GetEnvironmentVariable(envKey);

            // Nếu không có trong biến môi trường, lấy từ configuration
            return !string.IsNullOrEmpty(envValue)
                ? envValue
                : configuration[configKey] ?? string.Empty;
        }
    }
}