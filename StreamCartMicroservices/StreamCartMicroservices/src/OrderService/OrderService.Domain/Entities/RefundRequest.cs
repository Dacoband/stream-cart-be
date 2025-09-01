using OrderService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderService.Domain.Entities
{
    public class RefundRequest : BaseEntity
    {
        /// <summary>
        /// ID of the order being refunded
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// Tracking code for the return shipment
        /// </summary>
        public string? TrackingCode { get; set; }

        /// <summary>
        /// ID of the user who requested the refund
        /// </summary>
        public Guid RequestedByUserId { get; set; }

        /// <summary>
        /// Date and time when the refund was requested
        /// </summary>
        public DateTime RequestedAt { get; set; }

        /// <summary>
        /// Current status of the refund request
        /// </summary>
        public RefundStatus Status { get; set; }

        /// <summary>
        /// ID of the user who processed the refund
        /// </summary>
        public Guid? ProcessedByUserId { get; set; }

        /// <summary>
        /// Date and time when the refund was processed
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Amount to be refunded (excluding shipping fee)
        /// </summary>
        public decimal RefundAmount { get; set; }

        /// <summary>
        /// Shipping fee for the return
        /// </summary>
        public decimal ShippingFee { get; set; }

        /// <summary>
        /// Total amount including refund amount and shipping fee
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Collection of refund details
        /// </summary>
        private readonly List<RefundDetail> _refundDetails = new();
        public IReadOnlyList<RefundDetail> RefundDetails => _refundDetails.AsReadOnly();

        protected RefundRequest() : base() { }

        /// <summary>
        /// Creates a new refund request
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="requestedByUserId">User requesting the refund</param>
        /// <param name="shippingFee">Shipping fee for return</param>
        public RefundRequest(Guid orderId, Guid requestedByUserId, decimal shippingFee = 0) : base()
        {
            OrderId = orderId;
            RequestedByUserId = requestedByUserId;
            RequestedAt = DateTime.UtcNow;
            Status = RefundStatus.Created;
            ShippingFee = shippingFee;
            RefundAmount = 0;
            TotalAmount = 0;
            SetCreator(requestedByUserId.ToString());
        }

        /// <summary>
        /// Adds a refund detail item
        /// </summary>
        public void AddRefundDetail(RefundDetail refundDetail)
        {
            if (refundDetail == null)
                throw new ArgumentNullException(nameof(refundDetail));

            _refundDetails.Add(refundDetail);
            RecalculateAmounts();
        }

        /// <summary>
        /// Updates the refund status
        /// </summary>
        public void UpdateStatus(RefundStatus newStatus, string modifiedBy, Guid? processedBy = null)
        {
            Status = newStatus;
            SetModifier(modifiedBy);

            if (processedBy.HasValue)
            {
                ProcessedByUserId = processedBy;
                ProcessedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Sets the tracking code for return shipment
        /// </summary>
        public void SetTrackingCode(string trackingCode, string modifiedBy)
        {
            TrackingCode = trackingCode;
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Recalculates the refund amount and total amount
        /// </summary>
        private void RecalculateAmounts()
        {
            RefundAmount = _refundDetails.Sum(d => d.UnitPrice);
            TotalAmount = RefundAmount + ShippingFee;
        }

        /// <summary>
        /// Validates that the refund request is in a valid state
        /// </summary>
        public override bool IsValid()
        {
            return base.IsValid() &&
                   OrderId != Guid.Empty &&
                   RequestedByUserId != Guid.Empty &&
                   RefundAmount >= 0 &&
                   ShippingFee >= 0 &&
                   TotalAmount >= 0;
        }
    }
}