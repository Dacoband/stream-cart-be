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

            return variants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                SKU = v.SKU,
                Price = v.Price,
                FlashSalePrice = v.FlashSalePrice,
                Stock = v.Stock,
                CreatedAt = v.CreatedAt,
                CreatedBy = v.CreatedBy,
                LastModifiedAt = v.LastModifiedAt,
                LastModifiedBy = v.LastModifiedBy
            });
        }
    }
}