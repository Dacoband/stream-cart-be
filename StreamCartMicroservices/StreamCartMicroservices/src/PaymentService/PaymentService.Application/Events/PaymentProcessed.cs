using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Events
{
    /// <summary>
    /// Event emitted when a payment is processed (success or failure)
    /// </summary>
    public class PaymentProcessed
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? QrCode { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
