using ShopService.Domain.Enums;
using System;

namespace ShopService.Application.Events
{
    public class ShopStatusChanged
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;  
        public Guid AccountId { get; set; } 
        public DateTime Timestamp { get; set; } 
    }
}