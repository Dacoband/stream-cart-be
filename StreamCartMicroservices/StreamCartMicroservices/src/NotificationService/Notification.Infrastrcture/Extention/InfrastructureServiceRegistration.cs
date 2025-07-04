using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastrcture.Data;
using Notification.Infrastrcture.Interface;
using Notification.Infrastrcture.Repositories;
using Notification.Infrastrcture.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Infrastrcture.Extention
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDBSettings>(configuration.GetSection("MongoDB"));

            services.AddSingleton<NotificationDbContext>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            return services;
        }
    }
}
