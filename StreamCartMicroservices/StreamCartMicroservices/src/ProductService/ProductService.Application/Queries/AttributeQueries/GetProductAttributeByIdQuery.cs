using MediatR;
using ProductService.Application.DTOs.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.AttributeQueries
{
    public class GetProductAttributeByIdQuery : IRequest<ProductAttributeDto>
    {
        public Guid Id { get; set; }
    }
}
