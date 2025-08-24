    using CartService.Application.Command;
    using CartService.Application.DTOs;
    using CartService.Application.Handlers;
    using CartService.Application.Interfaces;
using CartService.Application.Query;
using CartService.Application.Services;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Shared.Common.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    namespace CartService.Application.Extensions
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
            services.AddScoped<IProductService, ProductService>();
                services.AddScoped<ICartService, CartService.Application.Services.CartService>();

                // Inside AddApplicationServices method
                services.AddScoped<IRequestHandler<AddToCartCommand, ApiResponse<CreateCartResponseDTO>>, AddToCartHandler>();
            services.AddScoped<IRequestHandler<GetMyCartQuery, ApiResponse<CartResponeDTO>>, GetMyCartHandler>();
            services.AddScoped<IRequestHandler<PreviewOrderQuery, ApiResponse<PreviewOrderResponseDTO>>, PreviewOrderHandler>();
            services.AddScoped<IRequestHandler<DeleteCartItemCommand, ApiResponse<bool>>, DeleteCartItemHandler>();
            services.AddScoped<IRequestHandler<UpdateCartItemCommand, ApiResponse<UpdateCartItemDTO>>, UpdateCartItemHandler>();

            return services;
            }
        }
    }
