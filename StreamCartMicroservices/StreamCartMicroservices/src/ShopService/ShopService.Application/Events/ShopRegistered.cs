using System;

namespace ShopService.Application.Events
{
    public class ShopRegistered
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public Guid AccountId { get; set; } 
        public DateTime RegistrationDate { get; set; }
    }
}
