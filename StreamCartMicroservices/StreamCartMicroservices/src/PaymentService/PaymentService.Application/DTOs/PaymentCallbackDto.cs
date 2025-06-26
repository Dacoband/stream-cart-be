using System;

namespace PaymentService.Application.DTOs
{
    public class PaymentCallbackDto
    {
        public bool IsSuccessful { get; set; }
        public string? QrCode { get; set; }
        public decimal? Fee { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RawResponse { get; set; }
    }
}