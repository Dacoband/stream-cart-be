using MediatR;
using ProductService.Application.DTOs.Products;
using System;

namespace ProductService.Application.Queries.DetailQueries
{
    public class GetProductDetailQuery : IRequest<ProductDetailDto>
    {
        public Guid ProductId { get; set; }
    }
}