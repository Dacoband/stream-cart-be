using System;
using PaymentService.Domain.Enums;
using ProductService.Domain.Enums;
using Shared.Common.Domain.Bases;

namespace PaymentService.Domain.Entities
{
    public class Payment : BaseEntity
    {
        /// <summary>
        /// ID of the related order
        /// </summary>
        public Guid OrderId { get; private set; }

        /// <summary>
        /// ID of the user who made the payment
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Payment amount
        /// </summary>
        public decimal Amount { get; private set; }

        /// <summary>
        /// Payment method used for the transaction
        /// </summary>
        public PaymentMethod PaymentMethod { get; private set; }

        /// <summary>
        /// Current status of the payment
        /// </summary>
        public PaymentStatus Status { get; private set; }

        /// <summary>
        /// QR code for the payment
        /// </summary>
        public string? QrCode { get; private set; }

        /// <summary>
        /// Payment processing fee
        /// </summary>
        public decimal Fee { get; private set; }

        /// <summary>
        /// Date and time when the payment was processed
        /// </summary>
        public DateTime? ProcessedAt { get; private set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Payment() : base() { }

        /// <summary>
        /// Creates a new payment
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="amount">Payment amount</param>
        /// <param name="paymentMethod">Payment method</param>
        public Payment(
            Guid orderId,
            Guid userId,
            decimal amount,
            PaymentMethod paymentMethod) : base()
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be greater than zero");

            OrderId = orderId;
            UserId = userId;
            Amount = amount;
            PaymentMethod = paymentMethod;
            Status = PaymentStatus.Pending;
            Fee = 0;
        }
        public void UpdateQrCode(string qrCode, string? modifier = null)
        {
            if (string.IsNullOrEmpty(qrCode))
                throw new ArgumentException("QR Code cannot be empty", nameof(qrCode));

            QrCode = qrCode;

            if (!string.IsNullOrWhiteSpace(modifier))
                SetModifier(modifier);
            else
                LastModifiedAt = DateTime.UtcNow;
        }
        public void MarkAsSuccessful(string qrCode, decimal fee = 0, string? modifier = null)
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException($"Cannot mark payment as successful. Current status: {Status}");

            QrCode = qrCode;
            Status = PaymentStatus.Paid;
            Fee = fee;
            ProcessedAt = DateTime.UtcNow;
            SetModifier(modifier ?? "System");
        }

        /// <summary>
        /// Marks the payment as failed
        /// </summary>
        /// <param name="modifier">User who processed the payment</param>
        public void MarkAsFailed(string? modifier = null)
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException($"Cannot mark payment as failed. Current status: {Status}");

            Status = PaymentStatus.Failed;
            ProcessedAt = DateTime.UtcNow;
            SetModifier(modifier ?? "System");
        }

        /// <summary>
        /// Refunds the payment
        /// </summary>
        /// <param name="modifier">User who refunded the payment</param>
        public void Refund(string? modifier = null)
        {
            if (Status != PaymentStatus.Paid)
                throw new InvalidOperationException($"Cannot refund payment. Current status: {Status}");

            Status = PaymentStatus.Refunded;
            SetModifier(modifier ?? "System");
        }

        /// <summary>
        /// Validates that the payment is in a valid state
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            return OrderId != Guid.Empty
                && UserId != Guid.Empty
                && Amount > 0;
        }
    }
}