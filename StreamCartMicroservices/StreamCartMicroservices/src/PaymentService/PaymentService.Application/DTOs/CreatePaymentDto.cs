using System;
using ProductService.Domain.Enums;

namespace PaymentService.Application.DTOs
{
    public class CreatePaymentDto
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? CreatedBy { get; set; }
    }
}