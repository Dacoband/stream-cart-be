using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Commands
{
    public class UpdatePaymentStatusCommand : IRequest<PaymentDto>
    {
        public Guid PaymentId { get; set; }
        public PaymentStatus NewStatus { get; set; }
        public string? QrCode { get; set; }
        public decimal? Fee { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
