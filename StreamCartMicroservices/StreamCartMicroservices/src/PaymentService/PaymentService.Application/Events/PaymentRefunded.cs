using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Events
{

    /// <summary>
    /// Event emitted when a payment is refunded
    /// </summary>
    public class PaymentRefunded
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string? RefundReason { get; set; }
        public DateTime RefundedAt { get; set; }
    }
}
