using System;

namespace OrderService.Application.Events
{
    public class OrderCreated
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public Guid AccountId { get; set; }
        public Guid ShopId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int ItemCount { get; set; }
    }
}