using System;

namespace PaymentService.Application.Events
{
    public class PaymentStatusChanged
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? QrCode { get; set; }  // Đã sửa từ TransactionId
        public DateTime UpdatedAt { get; set; }
    }
}