using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Services.Email;
using ShopService.Application.Interfaces;
using ShopService.Application.Services;
using System.Reflection;

namespace ShopService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký MediatR
            services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            
            // Đăng ký các services
            services.AddHttpClient<IAccountServiceClient, AccountServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:AccountService"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            });
            services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
            {
                var serviceUrl = configuration["ServiceUrls:ProductService"];
                if (!string.IsNullOrEmpty(serviceUrl))
                {
                    client.BaseAddress = new Uri(serviceUrl);
                }
            });
            services.AddScoped<IShopManagementService, ShopManagementService>();
            //services.AddHttpClient<IProductServiceClient, ProductServiceClient>();
            services.AddScoped<IAdminNotificationService, AdminNotificationService>();
            services.AddScoped<IEmailService, MailJetEmailService>();
            services.AddScoped<IMembershipService, MembershipService>();
            services.AddScoped<IShopMembershipService, ShopMembershipService>();

            return services;
        }
    }
}