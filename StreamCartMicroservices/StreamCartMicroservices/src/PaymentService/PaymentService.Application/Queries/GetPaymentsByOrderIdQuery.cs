using MediatR;
using PaymentService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Queries
{
    public class GetPaymentsByOrderIdQuery : IRequest<IEnumerable<PaymentDto>>
    {
        public Guid OrderId { get; set; }
    }
}
