using MediatR;
using ProductService.Application.DTOs.Variants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.VariantQueries
{
    public class GetProductVariantByIdQuery : IRequest<ProductVariantDto>
    {
        public Guid Id { get; set; }
    }
}
