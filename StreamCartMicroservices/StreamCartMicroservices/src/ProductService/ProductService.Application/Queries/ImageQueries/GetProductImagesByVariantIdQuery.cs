using MediatR;
using ProductService.Application.DTOs.Images;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.ImageQueries
{
    public class GetProductImagesByVariantIdQuery : IRequest<IEnumerable<ProductImageDto>>
    {
        public Guid VariantId { get; set; }
    }
}
