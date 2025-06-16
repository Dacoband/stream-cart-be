using MediatR;
using ProductService.Application.DTOs.Variants;
using System;
using System.Collections.Generic;

namespace ProductService.Application.Queries.VariantQueries
{
    public class GetProductVariantByIdQuery : IRequest<ProductVariantDto>
    {
        public Guid Id { get; set; }
    }

    public class GetAllProductVariantsQuery : IRequest<IEnumerable<ProductVariantDto>>
    {
    }

    public class GetVariantsByProductIdQuery : IRequest<IEnumerable<ProductVariantDto>>
    {
        public Guid ProductId { get; set; }
    }
}