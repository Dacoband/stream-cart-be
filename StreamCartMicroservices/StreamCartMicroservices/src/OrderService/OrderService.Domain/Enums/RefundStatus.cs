namespace OrderService.Domain.Enums
{
    /// <summary>
    /// Represents the current status of a refund request
    /// </summary>
    public enum RefundStatus
    {
        /// <summary>
        /// Refund request has been created (initial status)
        /// </summary>
        Created = 0,

        /// <summary>
        /// Refund request has been confirmed
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// Return package has been packed
        /// </summary>
        Packed = 2,

        /// <summary>
        /// Return package is on delivery
        /// </summary>
        OnDelivery = 3,

        /// <summary>
        /// Return package has been delivered
        /// </summary>
        Delivered = 4,

        /// <summary>
        /// Refund process completed
        /// </summary>
        Completed = 5,

        /// <summary>
        /// Refund has been processed and money returned
        /// </summary>
        Refunded = 6,

        /// <summary>
        /// Refund request has been rejected
        /// </summary>
        Rejected = 7
    }
}