using System;

namespace ShopService.Application.DTOs.Dashboard
{
    public class LivestreamOrdersDTO
    {
        public int TotalLivestreamOrders { get; set; }
        public int TotalNonLivestreamOrders { get; set; }
        public decimal LivestreamRevenue { get; set; }
        public decimal NonLivestreamRevenue { get; set; }
        public LivestreamOrderStat[] LivestreamStats { get; set; } = Array.Empty<LivestreamOrderStat>();
    }

    public class LivestreamOrderStat
    {
        public Guid LivestreamId { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
    
}