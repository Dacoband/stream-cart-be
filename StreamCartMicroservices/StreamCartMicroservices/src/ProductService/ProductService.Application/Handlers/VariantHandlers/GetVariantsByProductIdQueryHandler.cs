using MediatR;
using ProductService.Application.DTOs.Variants;
using ProductService.Application.Queries.VariantQueries;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.VariantHandlers
{
    public class GetVariantsByProductIdQueryHandler : IRequestHandler<GetVariantsByProductIdQuery, IEnumerable<ProductVariantDto>>
    {
        private readonly IProductVariantRepository _variantRepository;

        public GetVariantsByProductIdQueryHandler(IProductVariantRepository variantRepository)
        {
            _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));
        }

        public async Task<IEnumerable<ProductVariantDto>> Handle(GetVariantsByProductIdQuery request, CancellationToken cancellationToken)
        {
            var variants = await _variantRepository.GetByProductIdAsync(request.ProductId);

            return variants.Select(v =>
            {
                decimal finalPrice = v.FlashSalePrice.HasValue && v.FlashSalePrice.Value > 0
                    ? v.FlashSalePrice.Value
                    : v.Price;

                return new ProductVariantDto
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    SKU = v.SKU,
                    Price = v.Price,
                    FinalPrice = finalPrice,
                    FlashSalePrice = v.FlashSalePrice.HasValue ? ((v.Price - v.FlashSalePrice.Value) / v.Price) * 100 : 0,
                    Stock = v.Stock,
                    CreatedAt = v.CreatedAt,
                    CreatedBy = v.CreatedBy,
                    LastModifiedAt = v.LastModifiedAt,
                    LastModifiedBy = v.LastModifiedBy
                };
            });
        }
    }
}