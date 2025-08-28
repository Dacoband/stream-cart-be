using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Commands.AttributeCommands;
using ProductService.Application.Commands.AttributeValueCommands;
using ProductService.Application.Commands.CategoryCommands;
using ProductService.Application.Commands.CombinationCommands;
using ProductService.Application.Commands.FlashSaleCommands;
using ProductService.Application.Commands.ImageCommands;
using ProductService.Application.Commands.VariantCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.DTOs.Category;
using ProductService.Application.DTOs.Combinations;
using ProductService.Application.DTOs.FlashSale;
using ProductService.Application.DTOs.Images;
using ProductService.Application.DTOs.Products;
using ProductService.Application.DTOs.Variants;
using ProductService.Application.Handlers.AttributeHandlers;
using ProductService.Application.Handlers.AttributeValueHandlers;
using ProductService.Application.Handlers.CategoryHandlers;
using ProductService.Application.Handlers.CombinationHandlers;
using ProductService.Application.Handlers.DetailHandlers;
using ProductService.Application.Handlers.FlashSaleHandlers;
using ProductService.Application.Handlers.ImageHandlers;
using ProductService.Application.Handlers.VariantHandlers;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.CategoryQueries;
using ProductService.Application.Queries.DetailQueries;
using ProductService.Application.Queries.FlashSaleQueries;
using ProductService.Application.Queries.ImageQueries;
using ProductService.Application.Services;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using Shared.Common.Services.Email;
using System.Reflection;

namespace ProductService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký MediatR handlers
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });
            services.AddHttpClient<IShopServiceClient, ShopServiceClient>(client =>
            {
                var baseUrl = configuration["ServiceUrls:ShopService"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            });
            // Inside AddApplicationServices method
            services.AddScoped<IRequestHandler<CreateProductVariantCommand, DTOs.Variants.ProductVariantDto>, CreateProductVariantCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateProductVariantCommand, DTOs.Variants.ProductVariantDto>, UpdateProductVariantCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteProductVariantCommand, bool>, DeleteProductVariantCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateVariantStockCommand, DTOs.Variants.ProductVariantDto>, UpdateVariantStockCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateVariantPriceCommand, DTOs.Variants.ProductVariantDto>, UpdateVariantPriceCommandHandler>();
            services.AddScoped<IRequestHandler<BulkUpdateVariantStockCommand, bool>, BulkUpdateVariantStockCommandHandler>();

            services.AddScoped<IRequestHandler<CreateProductAttributeCommand, DTOs.Attributes.ProductAttributeDto>, CreateProductAttributeCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateProductAttributeCommand, DTOs.Attributes.ProductAttributeDto>, UpdateProductAttributeCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteProductAttributeCommand, bool>, DeleteProductAttributeCommandHandler>();

            services.AddScoped<IRequestHandler<CreateAttributeValueCommand, AttributeValueDto>, CreateAttributeValueCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateAttributeValueCommand, AttributeValueDto>, UpdateAttributeValueCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteAttributeValueCommand, bool>, DeleteAttributeValueCommandHandler>();

            services.AddScoped<IRequestHandler<CreateProductCombinationCommand, ProductCombinationDto>, CreateProductCombinationCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateProductCombinationCommand, ProductCombinationDto>, UpdateProductCombinationCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteProductCombinationCommand, bool>, DeleteProductCombinationCommandHandler>();
            services.AddScoped<IRequestHandler<GenerateProductCombinationsCommand, bool>, GenerateProductCombinationsCommandHandler>();
            // Image handlers
            services.AddScoped<IRequestHandler<UploadProductImageCommand, DTOs.Images.ProductImageDto>, UploadProductImageCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateProductImageCommand, DTOs.Images.ProductImageDto>, UpdateProductImageCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteProductImageCommand, bool>, DeleteProductImageCommandHandler>();
            services.AddScoped<IRequestHandler<SetPrimaryImageCommand, bool>, SetPrimaryImageCommandHandler>();
            services.AddScoped<IRequestHandler<ReorderProductImagesCommand, bool>, ReorderProductImagesCommandHandler>();

            services.AddScoped<IRequestHandler<GetAllProductImagesQuery, IEnumerable<DTOs.Images.ProductImageDto>>, GetAllProductImagesQueryHandler>();
            services.AddScoped<IRequestHandler<GetProductImageByIdQuery, DTOs.Images.ProductImageDto>, GetProductImageByIdQueryHandler>();
            services.AddScoped<IRequestHandler<GetProductImagesByProductIdQuery, IEnumerable<DTOs.Images.ProductImageDto>>, GetProductImagesByProductIdQueryHandler>();
            services.AddScoped<IRequestHandler<GetProductImagesByVariantIdQuery, IEnumerable<DTOs.Images.ProductImageDto>>, GetProductImagesByVariantIdQueryHandler>();
            services.AddScoped<IRequestHandler<GetProductDetailQuery, ProductDetailDto>, GetProductDetailQueryHandler>();
            services.AddScoped<IProductService, ProductManagementService>();
            services.AddScoped<IAttributeValueService, AttributeValueService>();
            services.AddScoped<IProductAttributeService, AttributeService>();
            services.AddScoped<IProductCombinationService, CombinationService>();
            services.AddScoped<IProductVariantService, ProductVariantService>();
            services.AddScoped<IProductImageService, ProductImageService>();
            services.AddScoped<IAccountCLientService,AccountClientService>();
            services.AddScoped<IEmailService, MailJetEmailService>();
            //Category 
            services.AddScoped<ICategoryService,CategoryService>();
            services.AddScoped<IRequestHandler<CreateCategoryCommand, ApiResponse<Category>>,CreateCategoryHandler>();
            services.AddScoped<IRequestHandler<GetAllCategoryQuery, ApiResponse<ListCategoryDTO>>, GetAllCategoryHanlder>();
            services.AddScoped<IRequestHandler<GetDetailCategoryQuery, ApiResponse<CategoryDetailDTO>>, GetDetailCategoryHandler>();
            services.AddScoped<IRequestHandler<UpdateCategoryCommand, ApiResponse<Category>>, UpdateCategoryHandler>();
            services.AddScoped<IRequestHandler<DeleteCategoryCommand, ApiResponse<bool>>, DeleteCategoryHandler>();
            services.AddScoped<IFlashSaleService, FlashSaleService>();
            //services.AddScoped<IRequestHandler<CreateFlashSaleCommand, ApiResponse<List<DetailFlashSaleDTO>>>, CreateFlashSaleHandle>();
            services.AddScoped<IRequestHandler<UpdateFlashSaleCommand, ApiResponse<DetailFlashSaleDTO>>, UpdateFlashSaleHandler>();
            services.AddScoped<IRequestHandler<GetAllFlashSaleQuery, ApiResponse<List<DetailFlashSaleDTO>>>, GetAllFlashSaleHandler>();
            services.AddScoped<IRequestHandler<GetDetailFlashSaleQuery, ApiResponse<DetailFlashSaleDTO>>, GetDetailFlashSaleHandler>();
            services.AddScoped<IRequestHandler<DeleteFlashSaleCommand, ApiResponse<bool>>, DeleteFlashSaleHandler>();
            return services;
        }
    }
}