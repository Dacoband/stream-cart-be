using OrderService.Domain.Enums;
using System;

namespace OrderService.Application.Events
{
    public class OrderStatusChanged
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public Guid AccountId { get; set; }
        public Guid ShopId { get; set; }
        public OrderStatus PreviousStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
    }
}