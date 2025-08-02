using PaymentService.Application.DTOs;
using ProductService.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Commands
{
    public class CreatePaymentCommand : IRequest<PaymentDto>
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? CreatedBy { get; set; }
        public string? QrCode { get; set; } 

    }
}
