namespace OrderService.Domain.Enums
{
    /// <summary>
    /// Represents the current status of an order
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Order has been created but not yet processed
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Order is being processed by the shop
        /// </summary>
        Processing = 1,
        
        /// <summary>
        /// Order has been shipped to the customer
        /// </summary>
        Shipped = 2,
        
        /// <summary>
        /// Order has been delivered to the customer
        /// </summary>
        Delivered = 3,
        
        /// <summary>
        /// Order has been cancelled
        /// </summary>
        Cancelled = 4,
        
        /// <summary>
        /// Order has been returned by the customer
        /// </summary>
        Returned = 5
    }
}