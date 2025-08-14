using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.ProductComands
{
    public class AcctivateProductCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
