using MediatR;
using ProductService.Application.Commands.VariantCommands;
using ProductService.Application.DTOs.Variants;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.VariantHandlers
{
    public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, ProductVariantDto>
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductRepository _productRepository;

        public CreateProductVariantCommandHandler(
            IProductVariantRepository variantRepository,
            IProductRepository productRepository)
        {
            _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<ProductVariantDto> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
        {
            // Check if product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId.ToString());
            if (product == null)
            {
                throw new ApplicationException($"Product with ID {request.ProductId} not found");
            }

            // Check if SKU is unique
            if (!string.IsNullOrWhiteSpace(request.SKU) && !await _variantRepository.IsSkuUniqueAsync(request.SKU))
            {
                throw new ApplicationException($"SKU '{request.SKU}' already exists");
            }

            // Ensure SKU is not null or empty
            var sku = request.SKU ?? throw new ArgumentNullException(nameof(request.SKU), "SKU cannot be null");

            // Ensure CreatedBy is not null or empty
            var createdBy = request.CreatedBy ?? "system";

            // Create the variant
            var variant = new ProductVariant(
                request.ProductId,
                sku,
                request.Price,
                request.Stock,
                createdBy);
            variant.Length = request.Length;
            variant.Weight = request.Weight;
            variant.Width = request.Width;
            variant.Height = request.Height;

            // Set flash sale price if present
            if (request.FlashSalePrice.HasValue)
            {
                variant.UpdatePrice(request.Price, request.FlashSalePrice);
            }

            // Save to database
            await _variantRepository.InsertAsync(variant);

            // Return DTO
            return new ProductVariantDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                SKU = variant.SKU,
                Price = variant.Price,
                FlashSalePrice = variant.FlashSalePrice,
                Stock = variant.Stock,
                CreatedAt = variant.CreatedAt,
                CreatedBy = variant.CreatedBy,
                LastModifiedAt = variant.LastModifiedAt,
                LastModifiedBy = variant.LastModifiedBy
            };
        }
    }
}