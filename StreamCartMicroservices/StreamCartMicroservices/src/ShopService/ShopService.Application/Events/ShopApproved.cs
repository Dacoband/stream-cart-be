using System;

namespace ShopService.Application.Events
{
    public class ShopApproved
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public Guid AccountId { get; set; } 
        public DateTime ApprovalDate { get; set; }
    }
}