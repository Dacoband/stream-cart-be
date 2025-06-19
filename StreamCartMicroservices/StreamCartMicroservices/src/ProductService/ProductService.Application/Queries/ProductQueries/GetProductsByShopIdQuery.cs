using MediatR;
using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.ProductQueries
{
    public class GetProductsByShopIdQuery : IRequest<IEnumerable<ProductDto>>
    {
        public Guid ShopId { get; set; }
        public bool ActiveOnly { get; set; }
    }
}
