using MediatR;
using ProductService.Application.DTOs.Variants;
using ProductService.Application.Queries.VariantQueries;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.VariantHandlers
{
    public class GetProductVariantById1QueryHandler : IRequestHandler<GetProductVariantById1Query, ProductVariantDto1>
    {
        private readonly IProductVariantRepository _variantRepository;

        public GetProductVariantById1QueryHandler(IProductVariantRepository variantRepository)
        {
            _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));
        }

        public async Task<ProductVariantDto1> Handle(GetProductVariantById1Query request, CancellationToken cancellationToken)
        {
            var variant = await _variantRepository.GetByIdAsync(request.Id.ToString());
            if (variant == null)
            {
                throw new InvalidOperationException($"Product variant with ID {request.Id} not found.");
            }
            var finalPrice = variant.Price;
            if (variant.FlashSalePrice.HasValue && variant.FlashSalePrice.Value > 0)
            {
                finalPrice = variant.FlashSalePrice.Value;

            }
          
            return new ProductVariantDto1
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                SKU = variant.SKU,
                Price = variant.Price,
                FlashSalePrice = variant.FlashSalePrice.HasValue ? ((variant.Price - variant.FlashSalePrice.Value) / variant.Price) * 100 : 0,
                FinalPrice = finalPrice,
                Stock = variant.Stock,
                CreatedAt = variant.CreatedAt,
                CreatedBy = variant.CreatedBy,
                LastModifiedAt = variant.LastModifiedAt,
                LastModifiedBy = variant.LastModifiedBy,
                Weight = variant.Weight,
                Length = variant.Length,
                Width = variant.Width,
                Height = variant.Height
            };
        }
    }
}