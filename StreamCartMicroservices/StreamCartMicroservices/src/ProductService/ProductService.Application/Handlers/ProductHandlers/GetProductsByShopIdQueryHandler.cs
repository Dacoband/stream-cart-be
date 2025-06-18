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

        public GetProductsByShopIdQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductDto>> Handle(GetProductsByShopIdQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetByShopIdAsync(request.ShopId);

            // Lọc theo trạng thái nếu cần
            if (request.ActiveOnly)
            {
                products = products.Where(p => p.IsActive);
            }

            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                ProductName = p.ProductName,
                Description = p.Description,
                SKU = p.SKU,
                CategoryId = p.CategoryId,
                BasePrice = p.BasePrice,
                DiscountPrice = p.DiscountPrice,
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
            }).ToList();
        }
    }
}