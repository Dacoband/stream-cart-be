using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.AttributeCommands
{
    public class DeleteProductAttributeCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? DeletedBy { get; set; }
    }
}
