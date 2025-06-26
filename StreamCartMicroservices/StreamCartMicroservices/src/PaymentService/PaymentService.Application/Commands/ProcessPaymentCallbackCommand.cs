using MediatR;
using PaymentService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Commands
{
    public class ProcessPaymentCallbackCommand : IRequest<PaymentDto>
    {
        public Guid PaymentId { get; set; }
        public bool IsSuccessful { get; set; }
        public string? QrCode { get; set; }
        public decimal? Fee { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RawResponse { get; set; }
    }
}
