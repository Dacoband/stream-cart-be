using MediatR;
using ProductService.Application.DTOs;
using ProductService.Application.Queries;
using ProductService.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers
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
               // LivestreamId = p.LivestreamId,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy,
                LastModifiedAt = p.LastModifiedAt,
                LastModifiedBy = p.LastModifiedBy
            }).ToList();
        }
    }
}