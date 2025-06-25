using System;

namespace OrderService.Application.Events
{
    public class PaymentCompleted
    {

        public Guid OrderId { get; set; }

        public string TransactionId { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public DateTime PaymentDate { get; set; }

        public string AdditionalInfo { get; set; } = string.Empty;
    }
}