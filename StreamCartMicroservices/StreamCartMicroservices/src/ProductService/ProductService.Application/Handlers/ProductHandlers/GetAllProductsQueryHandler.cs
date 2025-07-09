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
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetAllProductsQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetAllAsync();

            // Lọc sản phẩm theo trạng thái nếu cần
            if (request.ActiveOnly)
            {
                products = products.Where(p => p.IsActive && !p.IsDeleted);
            }
            else
            {
                products = products.Where(p => !p.IsDeleted);
            }

            return products.Select(p =>
            {
            decimal finalPrice = p.BasePrice;
            if (p.DiscountPrice.HasValue && p.DiscountPrice.Value > 0)
            {
                // Apply discount as a percentage of original price
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
                    // LivestreamId = p.LivestreamId,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy,
                    LastModifiedAt = p.LastModifiedAt,
                    LastModifiedBy = p.LastModifiedBy
                };
            }).ToList();
        }
    }
}