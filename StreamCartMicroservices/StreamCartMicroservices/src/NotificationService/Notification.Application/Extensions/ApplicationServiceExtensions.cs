using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Command;
using Notification.Application.DTOs;
using Notification.Application.Handlers;
using Notification.Application.Interfaces;
using Notification.Application.Queries;
using Notification.Application.Services;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            //Register MediatR
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            // Register services

            services.AddScoped<INotificationService, NotificationService>();
            // Inside AddApplicationServices method

            services.AddScoped<IRequestHandler<GetMyNotificationQuery, ApiResponse<ListNotificationDTO>>, GetMyNotificationHandler>();
            services.AddScoped<IRequestHandler<MarkAsRead, ApiResponse<bool>>, MarkAsReadHandler>();


            return services;
        }
    }
}
