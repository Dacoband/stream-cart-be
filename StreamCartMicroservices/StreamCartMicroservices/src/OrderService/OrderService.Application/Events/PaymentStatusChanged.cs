using OrderService.Domain.Enums;
using System;

namespace OrderService.Application.Events
{
    public class PaymentStatusChanged
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public PaymentStatus PreviousStatus { get; set; }
        public PaymentStatus NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}