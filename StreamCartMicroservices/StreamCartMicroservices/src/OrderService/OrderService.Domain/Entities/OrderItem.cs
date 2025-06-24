using System;
using Shared.Common.Domain.Bases;

namespace OrderService.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        #region Properties
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal DiscountAmount { get; private set; }

        /// <summary>
        /// Total price for this item (Quantity * UnitPrice - DiscountAmount)
        /// </summary>
        public decimal TotalPrice { get; private set; }
        public string Notes { get; private set; }
        public Guid? RefundRequestId { get; private set; }
        public Guid? VariantId { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }

        #endregion

        #region Constructors
        protected OrderItem() { }

        public OrderItem(
            Guid orderId, 
            Guid productId, 
            int quantity, 
            decimal unitPrice, 
            string notes = "", 
            Guid? variantId = null, 
            decimal discountAmount = 0)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
            
            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));
            
            if (discountAmount < 0)
                throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));
            
            if (discountAmount > unitPrice * quantity)
                throw new ArgumentException("Discount cannot exceed total price", nameof(discountAmount));
            
            OrderId = orderId;
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Notes = notes ?? string.Empty;
            VariantId = variantId;
            DiscountAmount = discountAmount;

            CalculateTotalPrice();
        }

        #endregion

        #region Domain Methods

        /// <summary>
        /// Updates the quantity of this order item
        /// </summary>
        public void UpdateQuantity(int quantity, string modifiedBy)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
            
            Quantity = quantity;
            
            if (DiscountAmount > UnitPrice * quantity)
                DiscountAmount = UnitPrice * quantity;
                
            CalculateTotalPrice();
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Updates the unit price of this order item
        /// </summary>
        public void UpdateUnitPrice(decimal unitPrice, string modifiedBy)
        {
            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));
            
            UnitPrice = unitPrice;
            
            if (DiscountAmount > UnitPrice * Quantity)
                DiscountAmount = UnitPrice * Quantity;
                
            CalculateTotalPrice();
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Applies a discount to this order item
        /// </summary>
        public void ApplyDiscount(decimal discountAmount, string modifiedBy)
        {
            if (discountAmount < 0)
                throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));
            
            if (discountAmount > UnitPrice * Quantity)
                throw new ArgumentException("Discount cannot exceed total price", nameof(discountAmount));
            
            DiscountAmount = discountAmount;
            CalculateTotalPrice();
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Updates the notes for this order item
        /// </summary>
        public void UpdateNotes(string notes, string modifiedBy)
        {
            Notes = notes ?? string.Empty;
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Links this item to a refund request
        /// </summary>
        public void LinkToRefundRequest(Guid refundRequestId, string modifiedBy)
        {
            RefundRequestId = refundRequestId;
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Removes the link to a refund request
        /// </summary>
        public void RemoveRefundRequestLink(string modifiedBy)
        {
            RefundRequestId = null;
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Updates the variant ID for this order item
        /// </summary>
        public void UpdateVariant(Guid? variantId, string modifiedBy)
        {
            VariantId = variantId;
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Calculates the total price based on quantity, unit price and discount
        /// </summary>
        private void CalculateTotalPrice()
        {
            TotalPrice = (Quantity * UnitPrice) - DiscountAmount;
        }

        #endregion
    }
}