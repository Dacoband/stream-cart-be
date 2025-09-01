using Shared.Common.Domain.Bases;
using System;

namespace OrderService.Domain.Entities
{
    public class RefundDetail : BaseEntity
    {
        /// <summary>
        /// ID of the order item being refunded
        /// </summary>
        public Guid OrderItemId { get; set; }

        /// <summary>
        /// ID of the refund request this detail belongs to
        /// </summary>
        public Guid RefundRequestId { get; set; }

        /// <summary>
        /// Reason for the refund
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// URL of the image evidence for refund
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Unit price from the order item's total price
        /// </summary>
        public decimal UnitPrice { get; set; }

        protected RefundDetail() : base() { }

        /// <summary>
        /// Creates a new refund detail
        /// </summary>
        /// <param name="orderItemId">Order item ID</param>
        /// <param name="refundRequestId">Refund request ID</param>
        /// <param name="reason">Reason for refund</param>
        /// <param name="unitPrice">Unit price from order item</param>
        /// <param name="imageUrl">Optional image URL</param>
        public RefundDetail(Guid orderItemId, Guid refundRequestId, string reason, decimal unitPrice, string? imageUrl = null) : base()
        {
            OrderItemId = orderItemId;
            RefundRequestId = refundRequestId;
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            UnitPrice = unitPrice;
            ImageUrl = imageUrl;
        }

        /// <summary>
        /// Updates the reason for refund
        /// </summary>
        public void UpdateReason(string reason, string modifiedBy)
        {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Sets the image URL for evidence
        /// </summary>
        public void SetImageUrl(string imageUrl, string modifiedBy)
        {
            ImageUrl = imageUrl;
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Validates that the refund detail is in a valid state
        /// </summary>
        public override bool IsValid()
        {
            return base.IsValid() &&
                   OrderItemId != Guid.Empty &&
                   RefundRequestId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(Reason) &&
                   UnitPrice >= 0;
        }
    }
}