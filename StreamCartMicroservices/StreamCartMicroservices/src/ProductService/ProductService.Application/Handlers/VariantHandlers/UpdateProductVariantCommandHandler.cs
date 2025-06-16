using MediatR;
using ProductService.Application.Commands.VariantCommands;
using ProductService.Application.DTOs.Variants;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.VariantHandlers
{
    public class UpdateProductVariantCommandHandler : IRequestHandler<UpdateProductVariantCommand, ProductVariantDto>
    {
        private readonly IProductVariantRepository _variantRepository;

        public UpdateProductVariantCommandHandler(IProductVariantRepository variantRepository)
        {
            _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));
        }

        public async Task<ProductVariantDto> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
        {
            var variant = await _variantRepository.GetByIdAsync(request.Id.ToString());
            if (variant == null)
            {
                throw new ApplicationException($"Product variant with ID {request.Id} not found");
            }

            // Check if SKU is unique if it's changed
            if (!string.IsNullOrWhiteSpace(request.SKU) && request.SKU != variant.SKU &&
                !await _variantRepository.IsSkuUniqueAsync(request.SKU, request.Id))
            {
                throw new ApplicationException($"SKU '{request.SKU}' already exists");
            }

            // Update SKU
            variant.UpdateSKU(request.SKU);

            // Update price and flash sale price
            variant.UpdatePrice(request.Price, request.FlashSalePrice);

            // Update stock
            variant.UpdateStock(request.Stock);

            // Set updater
            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                variant.SetUpdatedBy(request.UpdatedBy);
            }

            // Save to database
            await _variantRepository.ReplaceAsync(variant.Id.ToString(), variant);

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