using MediatR;
using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.ProductQueries
{
    public class GetBestSellingProductsQuery : IRequest<IEnumerable<ProductDto>>
    {
        public int Count { get; set; } = 10;
        public Guid? ShopId { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
