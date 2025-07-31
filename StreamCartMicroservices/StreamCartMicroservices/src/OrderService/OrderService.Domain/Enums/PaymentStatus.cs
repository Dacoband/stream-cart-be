namespace OrderService.Domain.Enums
{
    /// <summary>
    /// Represents the current payment status of an order
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Payment is pending
        /// </summary>
        pending = 0,
        
        /// <summary>
        /// Payment has been successfully processed
        /// </summary>
        paid = 1,
        
        /// <summary>
        /// Payment has failed
        /// </summary>
        failed = 2,
        
        /// <summary>
        /// Payment has been refunded
        /// </summary>
        refunded = 3,
        
        /// <summary>
        /// Payment has been partially refunded
        /// </summary>
        partiallyRefunded = 4
    }
}