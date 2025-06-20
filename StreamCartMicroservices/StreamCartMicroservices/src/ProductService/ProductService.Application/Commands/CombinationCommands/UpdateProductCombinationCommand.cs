using MediatR;
using ProductService.Application.DTOs.Combinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.CombinationCommands
{
    public class UpdateProductCombinationCommand : IRequest<ProductCombinationDto>
    {
        public Guid CurrentVariantId { get; set; }
        public Guid CurrentAttributeValueId { get; set; }
        public Guid NewAttributeValueId { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
