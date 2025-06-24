using OrderService.Domain.Enums;
using System;

namespace OrderService.Application.Events
{
    public class OrderCancelRequest
    {

        public Guid OrderId { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string CancellationReason { get; set; } = string.Empty;
        public OrderStatus PreviousStatus { get; set; }
    }
}