using MediatR;
using ProductService.Application.DTOs.Attributes;
using System;
using System.Collections.Generic;

namespace ProductService.Application.Queries.AttributeValueQueries
{
    public class GetAttributeValueByIdQuery : IRequest<AttributeValueDto>
    {
        public Guid Id { get; set; }
    }

    public class GetAllAttributeValuesQuery : IRequest<IEnumerable<AttributeValueDto>>
    {
    }

    public class GetAttributeValuesByAttributeIdQuery : IRequest<IEnumerable<AttributeValueDto>>
    {
        public Guid AttributeId { get; set; }
    }
}