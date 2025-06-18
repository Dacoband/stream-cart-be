using MediatR;
using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.ProductQueries
{
    public class GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>
    {
        public bool ActiveOnly { get; set; }
    }
}
