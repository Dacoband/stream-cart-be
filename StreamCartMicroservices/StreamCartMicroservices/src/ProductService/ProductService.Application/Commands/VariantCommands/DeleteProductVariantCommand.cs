using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.VariantCommands
{
    public class DeleteProductVariantCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? DeletedBy { get; set; }
    }
}
