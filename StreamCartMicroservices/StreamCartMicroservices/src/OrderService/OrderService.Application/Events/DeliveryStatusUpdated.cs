using System;

namespace OrderService.Application.Events
{
    public class DeliveryStatusUpdated
    {
        public Guid OrderId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Delivery status (e.g., "Shipped", "Delivered", "Failed")
        /// </summary>
        public string DeliveryStatus { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string StatusDetails { get; set; } = string.Empty;
    }
}