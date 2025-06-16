using MediatR;
using ProductService.Application.DTOs.Attributes;
using System;
using System.Collections.Generic;

namespace ProductService.Application.Queries.AttributeQueries
{
    public class GetProductAttributeByIdQuery : IRequest<ProductAttributeDto>
    {
        public Guid Id { get; set; }
    }

    public class GetAllProductAttributesQuery : IRequest<IEnumerable<ProductAttributeDto>>
    {
    }

    public class GetAttributesByProductIdQuery : IRequest<IEnumerable<ProductAttributeDto>>
    {
        public Guid ProductId { get; set; }
    }
}
