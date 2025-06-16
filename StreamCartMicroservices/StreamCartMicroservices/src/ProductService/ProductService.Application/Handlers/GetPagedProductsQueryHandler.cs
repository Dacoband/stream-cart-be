using MediatR;
using ProductService.Application.DTOs;
using ProductService.Application.Queries;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Domain.Bases;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers
{
    public class GetPagedProductsQueryHandler : IRequestHandler<GetPagedProductsQuery, PagedResult<ProductDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetPagedProductsQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<PagedResult<ProductDto>> Handle(GetPagedProductsQuery request, CancellationToken cancellationToken)
        {
            var pagedProducts = await _productRepository.GetPagedProductsAsync(
                request.PageNumber,
                request.PageSize,
                request.SortOption,
                request.ActiveOnly,
                request.ShopId,
                request.CategoryId);

            var productDtos = pagedProducts.Items.Select(p => new ProductDto
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

            // Create a new PagedResult using the request parameters instead of trying to access properties
            // that might not exist on the returned PagedResult<Product>
            return new PagedResult<ProductDto>(
                productDtos,
                pagedProducts.TotalCount,
                request.PageNumber,
                request.PageSize);
        }
    }
}