using MediatR;
using ProductService.Application.DTOs;
using ProductService.Application.Queries.ProductQueries;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Domain.Bases;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MassTransit.ValidationResultExtensions;

namespace ProductService.Application.Handlers.ProductHandlers
{
    public class GetPagedProductsQueryHandler : IRequestHandler<GetPagedProductsQuery, PagedResult<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;

        public GetPagedProductsQueryHandler(
            IProductRepository productRepository,
            IProductImageRepository productImageRepository)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
        }

        public async Task<PagedResult<ProductDto>> Handle(GetPagedProductsQuery request, CancellationToken cancellationToken)
        {
            var pagedProducts = await _productRepository.GetPagedProductsAsync(
                request.PageNumber,
                request.PageSize,
                request.SortOption,
                request.ActiveOnly,
                request.ShopId,
                request.CategoryId, request.InStockOnly);

            var productDtos = new List<ProductDto>();

            foreach (var p in pagedProducts.Items)
            {
                decimal finalPrice = p.BasePrice;
                if (p.DiscountPrice.HasValue && p.DiscountPrice.Value > 0)
                {
                    finalPrice = p.BasePrice * (1 - (p.DiscountPrice.Value / 100));
                }

                // Get primary image if exists
                var primaryImage = await _productImageRepository.GetPrimaryImageAsync(p.Id);
                string? primaryImageUrl = primaryImage?.ImageUrl;
                 finalPrice = (p.DiscountPrice.HasValue && p.DiscountPrice.Value > 0m)
    ? p.DiscountPrice.Value   // giá sau giảm (sale price)
    : p.BasePrice;

                // Tính % giảm an toàn
                decimal discountPercent = 0m;
                if (p.BasePrice > 0m && finalPrice < p.BasePrice)
                {
                    discountPercent = ((p.BasePrice - finalPrice) / p.BasePrice) * 100m;
                    // Làm tròn 2 chữ số thập phân (tuỳ bạn)
                    discountPercent = Math.Round(discountPercent, 2);

                    // Giới hạn [0, 100] để tránh lệch
                    if (discountPercent < 0m) discountPercent = 0m;
                    else if (discountPercent > 100m) discountPercent = 100m;
                }

                productDtos.Add(new ProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    SKU = p.SKU,
                    CategoryId = p.CategoryId,

                    BasePrice = p.BasePrice,
                    // ĐỔI: nếu DTO đang dùng tên DiscountPrice nhưng mang ý nghĩa phần trăm,
                    // thì gán discountPercent. (Khuyến nghị đổi tên thành DiscountPercent cho rõ)
                    DiscountPrice = discountPercent,

                    FinalPrice = finalPrice,
                    StockQuantity = p.StockQuantity,
                    ReserveStock = p.ReserveStock,
                    IsActive = p.IsActive,
                    Weight = p.Weight,
                    Length = p.Length,
                    Width = p.Width,
                    Height = p.Height,
                    HasVariant = p.HasVariant,
                    QuantitySold = p.QuantitySold,
                    ShopId = p.ShopId,
                    // LivestreamId = p.LivestreamId,
                    PrimaryImageUrl = primaryImageUrl,
                    HasPrimaryImage = primaryImage != null,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy,
                    LastModifiedAt = p.LastModifiedAt,
                    LastModifiedBy = p.LastModifiedBy
                });

            }

            // Create a new PagedResult using the request parameters
            return new PagedResult<ProductDto>(
                productDtos,
                pagedProducts.TotalCount,
                request.PageNumber,
                request.PageSize);
        }
    }
}