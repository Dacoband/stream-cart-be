using MediatR;
using ProductService.Application.Commands.VariantCommands;
using ProductService.Application.DTOs.Variants;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.VariantHandlers
{
    public class UpdateVariantStockCommandHandler : IRequestHandler<UpdateVariantStockCommand, ProductVariantDto>
    {
        private readonly IProductVariantRepository _variantRepository;

        public UpdateVariantStockCommandHandler(IProductVariantRepository variantRepository)
        {
            _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));
        }

        public async Task<ProductVariantDto> Handle(UpdateVariantStockCommand request, CancellationToken cancellationToken)
        {
            var variant = await _variantRepository.GetByIdAsync(request.Id.ToString());
            if (variant == null)
            {
                throw new ApplicationException($"Product variant with ID {request.Id} not found");
            }

            variant.UpdateStock(request.Stock);

            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                variant.SetUpdatedBy(request.UpdatedBy);
            }

            await _variantRepository.ReplaceAsync(variant.Id.ToString(), variant);

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