using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messaging.Event.OrderEvents
{
    public class OrderCreatedOrUpdatedEvent
    {
        public string OrderCode { get; set; }
        public List<string> UserId { get; set; }
        public string Message { get; set; }
        public string? OrderStatus { get; set; }
        public List<OrderItemInEvent>? OrderItems { get; set; }
        public double? ShopRate { get; set; } = 0;
        public double? UserRate { get; set; } = 0;
        public string? CreatedBy { get; set; }
        public string? ShopId { get; set; }
    }
    public class OrderItemInEvent { 
        public string ProductId { get; set; }
        public string? VariantId { get; set; }
        public int Quantity { get; set; }
      
    
    }
}
