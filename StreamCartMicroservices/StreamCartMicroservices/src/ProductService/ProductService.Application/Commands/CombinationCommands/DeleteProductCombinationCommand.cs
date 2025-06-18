using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.CombinationCommands
{
    public class DeleteProductCombinationCommand : IRequest<bool>
    {
        public Guid VariantId { get; set; }
        public Guid AttributeValueId { get; set; }
        public string? DeletedBy { get; set; }
    }
}
