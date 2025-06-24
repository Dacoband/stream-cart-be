using MediatR;
using PaymentService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Commands
{
    public class RefundPaymentCommand : IRequest<PaymentDto>
    {
        public Guid PaymentId { get; set; }
        public string? Reason { get; set; }
        public string? RequestedBy { get; set; }
    }
}
