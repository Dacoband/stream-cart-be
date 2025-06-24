using System;
using PaymentService.Domain.Enums;

namespace PaymentService.Application.Events
{
    /// <summary>
    /// Event emitted when a payment is created
    /// </summary>
    public class PaymentCreated
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}