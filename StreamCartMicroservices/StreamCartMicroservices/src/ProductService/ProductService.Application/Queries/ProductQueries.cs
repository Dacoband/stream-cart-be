using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;

namespace ProductService.Application.Queries
{
    // Lấy sản phẩm theo ID
    public class GetProductByIdQuery : IRequest<ProductDto>
    {
        public Guid Id { get; set; }
    }

    // Lấy tất cả sản phẩm
    public class GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>
    {
        public bool ActiveOnly { get; set; }
    }

    // Lấy sản phẩm phân trang
    public class GetPagedProductsQuery : IRequest<PagedResult<ProductDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ProductSortOption SortOption { get; set; } = ProductSortOption.DateCreatedDesc;
        public bool ActiveOnly { get; set; } = false;
        public Guid? ShopId { get; set; }
        public Guid? CategoryId { get; set; }
    }

    // Lấy sản phẩm của một shop
    public class GetProductsByShopIdQuery : IRequest<IEnumerable<ProductDto>>
    {
        public Guid ShopId { get; set; }
        public bool ActiveOnly { get; set; }
    }

    // Lấy sản phẩm của một danh mục
    public class GetProductsByCategoryIdQuery : IRequest<IEnumerable<ProductDto>>
    {
        public Guid CategoryId { get; set; }
        public bool ActiveOnly { get; set; }
    }

    // Lấy sản phẩm bán chạy nhất
    public class GetBestSellingProductsQuery : IRequest<IEnumerable<ProductDto>>
    {
        public int Count { get; set; } = 10;
        public Guid? ShopId { get; set; }
        public Guid? CategoryId { get; set; }
    }

    // Lấy sản phẩm của một livestream
    public class GetProductsByLivestreamIdQuery : IRequest<IEnumerable<ProductDto>>
    {
        public Guid LivestreamId { get; set; }
    }
}