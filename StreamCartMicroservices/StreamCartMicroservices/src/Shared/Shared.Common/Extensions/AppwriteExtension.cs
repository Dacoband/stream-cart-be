    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Shared.Common.Services.Appwrite;
    using Shared.Common.Services.Email;
    using Shared.Common.Settings;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Shared.Common.Extensions
    {
        public static class AppwriteExtension
        {
            public static IServiceCollection AddAppwriteServices(this IServiceCollection services, IConfiguration configuration)
            {
                var appwriteSettingsSection = configuration.GetSection($"{nameof(AppSettings.AppwriteSetting)}");
                services.Configure<AppwriteSetting>(appwriteSettingsSection);

                services.AddHttpClient<IAppwriteService,AppwriteService>();

                return services;
            }
        }
    }
