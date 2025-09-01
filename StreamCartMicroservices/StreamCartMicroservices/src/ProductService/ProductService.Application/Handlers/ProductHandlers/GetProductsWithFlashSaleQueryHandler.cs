using MediatR;
using ProductService.Application.DTOs;
using ProductService.Application.Queries.ProductQueries;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ProductHandlers
{
    public class GetProductsWithFlashSaleQueryHandler : IRequestHandler<GetProductsWithFlashSaleQuery, ApiResponse<IEnumerable<ProductDto>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;

        public GetProductsWithFlashSaleQueryHandler(
            IProductRepository productRepository,
            IProductImageRepository productImageRepository)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> Handle(GetProductsWithFlashSaleQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var products = await _productRepository.GetProductsHaveFlashSale();

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
                    if (p.DiscountPrice.HasValue && p.DiscountPrice.Value > 0)
                    {
                        finalPrice = p.DiscountPrice.Value;

                    }
                    else
                    {
                        finalPrice = p.BasePrice;
                    }
                    result.Add(new ProductDto
                    {
                        Id = p.Id,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        SKU = p.SKU,
                        CategoryId = p.CategoryId,
                        BasePrice = p.BasePrice,
                        DiscountPrice = (p.BasePrice - finalPrice) / 100,
                        FinalPrice = finalPrice,
                        StockQuantity = p.StockQuantity,
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

                return new ApiResponse<IEnumerable<ProductDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách sản phẩm có Flash Sale thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = "Lỗi khi lấy danh sách sản phẩm có Flash Sale: " + ex.Message
                };
            }
        }
    }
}