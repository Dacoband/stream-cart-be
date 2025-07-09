using MediatR;
using ProductService.Application.DTOs;
using ProductService.Application.Queries.ProductQueries;
using ProductService.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ProductHandlers
{
    public class GetProductsByCategoryIdQueryHandler : IRequestHandler<GetProductsByCategoryIdQuery, IEnumerable<ProductDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetProductsByCategoryIdQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductDto>> Handle(GetProductsByCategoryIdQuery request, CancellationToken cancellationToken)
        {
            var products = request.ActiveOnly
                ? await _productRepository.GetActiveByCategoryIdAsync(request.CategoryId)
                : await _productRepository.GetByCategoryIdAsync(request.CategoryId);

            return products.Select(p =>
            {
                decimal finalPrice = p.BasePrice;
                if (p.DiscountPrice.HasValue && p.DiscountPrice.Value > 0)
                {
                    finalPrice = p.BasePrice * (1 - (p.DiscountPrice.Value / 100));
                }

                return new ProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    SKU = p.SKU,
                    CategoryId = p.CategoryId,
                    BasePrice = p.BasePrice,
                    DiscountPrice = p.DiscountPrice,
                    FinalPrice = finalPrice,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    Weight = p.Weight,
                    Dimensions = p.Dimensions,
                    HasVariant = p.HasVariant,
                    QuantitySold = p.QuantitySold,
                    ShopId = p.ShopId,
                    //LivestreamId = p.LivestreamId,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy,
                    LastModifiedAt = p.LastModifiedAt,
                    LastModifiedBy = p.LastModifiedBy
                };
            }).ToList();
        }
    }
}