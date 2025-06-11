using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Services.Email;
using Shared.Common.Settings;
using System;

namespace Shared.Common.Extensions
{
    public static class EmailServiceExtensions
    {
        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            var emailSettingsSection = configuration.GetSection($"{nameof(AppSettings.EmailSettings)}");
            services.Configure<EmailSettings>(emailSettingsSection);

            services.AddHttpClient<IEmailService, MailJetEmailService>();

            return services;
        }
    }
}