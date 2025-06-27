using MediatR;
using PaymentService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Queries
{
    public class GetPaymentByIdQuery : IRequest<PaymentDto>
    {
        public Guid PaymentId { get; set; }
    }
}
