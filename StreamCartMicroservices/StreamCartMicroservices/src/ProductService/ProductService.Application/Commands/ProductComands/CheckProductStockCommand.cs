using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.ProductComands
{
    public class CheckProductStockCommand : IRequest<bool>
    {
        public Guid ProductId { get; set; }
        public int RequestedQuantity { get; set; }
    }
}
