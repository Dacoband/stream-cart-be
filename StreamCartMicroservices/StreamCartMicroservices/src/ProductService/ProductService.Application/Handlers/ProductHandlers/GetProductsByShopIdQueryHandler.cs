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
    public class GetProductsByShopIdQueryHandler : IRequestHandler<GetProductsByShopIdQuery, IEnumerable<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;

        public GetProductsByShopIdQueryHandler(
            IProductRepository productRepository,
            IProductImageRepository productImageRepository)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
        }

        public async Task<IEnumerable<ProductDto>> Handle(GetProductsByShopIdQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetByShopIdAsync(request.ShopId);

            // Lọc theo trạng thái nếu cần
            if (request.ActiveOnly)
            {
                products = products.Where(p => p.IsActive);
            }

            var result = new List<ProductDto>();

            foreach (var p in products)
            {
                decimal finalPrice = p.BasePrice;
                if (p.DiscountPrice.HasValue && p.DiscountPrice.Value > 0)
                {
                    finalPrice = p.BasePrice * (1 - (p.DiscountPrice.Value / 100));
                }

                // Get primary image if exists
                var primaryImage = await _productImageRepository.GetPrimaryImageAsync(p.Id);
                string? primaryImageUrl = primaryImage?.ImageUrl;

                result.Add(new ProductDto
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
                    PrimaryImageUrl = primaryImageUrl,
                    HasPrimaryImage = primaryImage != null,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy,
                    LastModifiedAt = p.LastModifiedAt,
                    LastModifiedBy = p.LastModifiedBy
                });
            }

            return result;
        }
    }
}