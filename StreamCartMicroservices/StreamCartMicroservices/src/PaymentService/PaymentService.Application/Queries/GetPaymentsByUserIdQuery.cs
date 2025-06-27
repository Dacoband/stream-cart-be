using MediatR;
using PaymentService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Queries
{
    public class GetPaymentsByUserIdQuery : IRequest<IEnumerable<PaymentDto>>
    {
        public Guid UserId { get; set; }
    }
}
