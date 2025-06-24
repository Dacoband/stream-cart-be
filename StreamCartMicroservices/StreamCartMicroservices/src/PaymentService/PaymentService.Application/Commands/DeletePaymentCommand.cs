using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Commands
{
    public class DeletePaymentCommand : IRequest<bool>
    {
        public Guid PaymentId { get; set; }
        public string? DeletedBy { get; set; }
    }
}
