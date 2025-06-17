using System;

namespace ShopService.Application.Events
{
    public class ShopRejected
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public Guid AccountId { get; set; }
        public DateTime RejectionDate { get; set; }
    }
}