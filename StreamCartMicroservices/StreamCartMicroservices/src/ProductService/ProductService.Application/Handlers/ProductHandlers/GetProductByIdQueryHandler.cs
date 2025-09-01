using MediatR;
using ProductService.Application.DTOs;
using ProductService.Application.Queries.ProductQueries;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ProductHandlers
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;

        public GetProductByIdQueryHandler(
            IProductRepository productRepository,
            IProductImageRepository productImageRepository)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
        }

        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id.ToString());

            if (product == null || product.IsDeleted)
            {
                return null;
            }

            decimal finalPrice = product.BasePrice;
            if (product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0)
            {
                finalPrice = product.BasePrice * (1 - (product.DiscountPrice.Value / 100));
            }

            // Get primary image if exists
            var primaryImage = await _productImageRepository.GetPrimaryImageAsync(product.Id);
            string? primaryImageUrl = primaryImage?.ImageUrl;
             finalPrice = (product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0m)
      ? product.DiscountPrice.Value   // giá sau giảm (sale price)
      : product.BasePrice;

            decimal discountPercent = 0m;
            if (product.BasePrice > 0m && finalPrice < product.BasePrice)
            {
                discountPercent = ((product.BasePrice - finalPrice) / product.BasePrice) * 100m;
                discountPercent = Math.Round(discountPercent, 2);
            }

            return new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Description = product.Description,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                BasePrice = product.BasePrice,

                // Nếu DTO field này thật sự là % giảm, gán discountPercent
                // Nên đổi tên thành DiscountPercent cho dễ hiểu
                DiscountPrice = discountPercent,

                FinalPrice = finalPrice,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                Weight = product.Weight,
                Length = product.Length,
                Width = product.Width,
                Height = product.Height,
                HasVariant = product.HasVariant,
                QuantitySold = product.QuantitySold,
                ShopId = product.ShopId,
                //LivestreamId = product.LivestreamId,
                PrimaryImageUrl = primaryImageUrl,
                HasPrimaryImage = primaryImage != null,
                CreatedAt = product.CreatedAt,
                CreatedBy = product.CreatedBy,
                LastModifiedAt = product.LastModifiedAt,
                LastModifiedBy = product.LastModifiedBy
            };

        }
    }
}