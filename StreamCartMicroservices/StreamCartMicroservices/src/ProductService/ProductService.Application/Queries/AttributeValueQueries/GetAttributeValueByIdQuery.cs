using MediatR;
using ProductService.Application.DTOs.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.AttributeValueQueries
{
    public class GetAttributeValueByIdQuery : IRequest<AttributeValueDto>
    {
        public Guid Id { get; set; }
    }
}
