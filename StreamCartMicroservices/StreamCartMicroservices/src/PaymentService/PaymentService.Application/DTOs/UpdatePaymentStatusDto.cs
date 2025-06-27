using System;
using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTOs
{
    public class UpdatePaymentStatusDto
    {
        public PaymentStatus NewStatus { get; set; }
        public string? QrCode { get; set; }
        public decimal? Fee { get; set; }
        public string? UpdatedBy { get; set; }
    }
}