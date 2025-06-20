using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.ProductQueries
{
    public class GetPagedProductsQuery : IRequest<PagedResult<ProductDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ProductSortOption SortOption { get; set; } = ProductSortOption.DateCreatedDesc;
        public bool ActiveOnly { get; set; } = false;
        public Guid? ShopId { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
