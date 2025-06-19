using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.VariantCommands
{
    public class CheckVariantStockCommand : IRequest<bool>
    {
        public Guid VariantId { get; set; }
        public int RequestedQuantity { get; set; }
    }
}
