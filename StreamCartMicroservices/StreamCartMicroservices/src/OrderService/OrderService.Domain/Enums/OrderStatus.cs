namespace OrderService.Domain.Enums
{
    /// <summary>
    /// Represents the current status of an order
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Order has been created but waiting for shop confirmation
        /// </summary>
        Waiting = 0,

        /// <summary>
        /// Order has been confirmed by shop and is being processed
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Order is being processed by the shop
        /// </summary>
        Processing = 2,

        /// <summary>
        /// Order has been shipped
        /// </summary>
        Shipped = 3,

        /// <summary>
        /// Order has been delivered
        /// </summary>
        Delivered = 4,

        /// <summary>
        /// Order has been cancelled
        /// </summary>
        Cancelled = 5,

        Packed = 6,
        OnDelivere = 7,
        Returning = 8,
        Refunded = 9,
        Completed = 10,

    }
}