using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Commands.AttributeCommands;
using ProductService.Application.Commands.AttributeValueCommands;
using ProductService.Application.Commands.CategoryCommands;
using ProductService.Application.Commands.CombinationCommands;
using ProductService.Application.Commands.ImageCommands;
using ProductService.Application.Commands.VariantCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.DTOs.Category;
using ProductService.Application.DTOs.Combinations;
using ProductService.Application.DTOs.Details;
using ProductService.Application.DTOs.Images;
using ProductService.Application.DTOs.Variants;
using ProductService.Application.Handlers.AttributeHandlers;
using ProductService.Application.Handlers.AttributeValueHandlers;
using ProductService.Application.Handlers.CategoryHandlers;
using ProductService.Application.Handlers.CombinationHandlers;
using ProductService.Application.Handlers.DetailHandlers;
using ProductService.Application.Handlers.ImageHandlers;
using ProductService.Application.Handlers.VariantHandlers;
using ProductService.Application.Interface;
using ProductService.Application.Queries.CategoryQueries;
using ProductService.Application.Queries.DetailQueries;
using ProductService.Application.Queries.ImageQueries;
using ProductService.Application.Services;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using System.Reflection;

namespace ProductService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Đăng ký MediatR handlers
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });
            // Inside AddApplicationServices method
            services.AddScoped<IRequestHandler<CreateProductVariantCommand, ProductVariantDto>, CreateProductVariantCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateProductVariantCommand, ProductVariantDto>, UpdateProductVariantCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteProductVariantCommand, bool>, DeleteProductVariantCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateVariantStockCommand, ProductVariantDto>, UpdateVariantStockCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateVariantPriceCommand, ProductVariantDto>, UpdateVariantPriceCommandHandler>();
            services.AddScoped<IRequestHandler<BulkUpdateVariantStockCommand, bool>, BulkUpdateVariantStockCommandHandler>();

            services.AddScoped<IRequestHandler<CreateProductAttributeCommand, ProductAttributeDto>, CreateProductAttributeCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateProductAttributeCommand, ProductAttributeDto>, UpdateProductAttributeCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteProductAttributeCommand, bool>, DeleteProductAttributeCommandHandler>();

            services.AddScoped<IRequestHandler<CreateAttributeValueCommand, AttributeValueDto>, CreateAttributeValueCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateAttributeValueCommand, AttributeValueDto>, UpdateAttributeValueCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteAttributeValueCommand, bool>, DeleteAttributeValueCommandHandler>();

            services.AddScoped<IRequestHandler<CreateProductCombinationCommand, ProductCombinationDto>, CreateProductCombinationCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateProductCombinationCommand, ProductCombinationDto>, UpdateProductCombinationCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteProductCombinationCommand, bool>, DeleteProductCombinationCommandHandler>();
            services.AddScoped<IRequestHandler<GenerateProductCombinationsCommand, bool>, GenerateProductCombinationsCommandHandler>();
            // Image handlers
            services.AddScoped<IRequestHandler<UploadProductImageCommand, ProductImageDto>, UploadProductImageCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateProductImageCommand, ProductImageDto>, UpdateProductImageCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteProductImageCommand, bool>, DeleteProductImageCommandHandler>();
            services.AddScoped<IRequestHandler<SetPrimaryImageCommand, bool>, SetPrimaryImageCommandHandler>();
            services.AddScoped<IRequestHandler<ReorderProductImagesCommand, bool>, ReorderProductImagesCommandHandler>();

            services.AddScoped<IRequestHandler<GetAllProductImagesQuery, IEnumerable<ProductImageDto>>, GetAllProductImagesQueryHandler>();
            services.AddScoped<IRequestHandler<GetProductImageByIdQuery, ProductImageDto>, GetProductImageByIdQueryHandler>();
            services.AddScoped<IRequestHandler<GetProductImagesByProductIdQuery, IEnumerable<ProductImageDto>>, GetProductImagesByProductIdQueryHandler>();
            services.AddScoped<IRequestHandler<GetProductImagesByVariantIdQuery, IEnumerable<ProductImageDto>>, GetProductImagesByVariantIdQueryHandler>();
            services.AddScoped<IRequestHandler<GetProductDetailQuery, ProductDetailDto>, GetProductDetailQueryHandler>();

            //Category 
            services.AddScoped<ICategoryService,CategoryService>();
            services.AddScoped<IRequestHandler<CreateCategoryCommand, ApiResponse<Category>>,CreateCategoryHandler>();
            services.AddScoped<IRequestHandler<GetAllCategoryQuery, ApiResponse<ICollection<CategoryDetailDTO>>>, GetAllCategoryHanlder>();
            services.AddScoped<IRequestHandler<GetDetailCategoryQuery, ApiResponse<CategoryDetailDTO>>, GetDetailCategoryHandler>();
            services.AddScoped<IRequestHandler<UpdateCategoryCommand, ApiResponse<Category>>, UpdateCategoryHandler>();
            services.AddScoped<IRequestHandler<DeleteCategoryCommand, ApiResponse<bool>>, DeleteCategoryHandler>();


            return services;
        }
    }
}