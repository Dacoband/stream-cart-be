using System;

namespace PaymentService.Application.DTOs
{
    public class RefundPaymentDto
    {
        public string Reason { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
    }
}