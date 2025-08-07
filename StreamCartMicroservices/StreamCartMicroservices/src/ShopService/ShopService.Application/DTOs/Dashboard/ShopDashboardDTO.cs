using System;
using System.Collections.Generic;

namespace ShopService.Application.DTOs.Dashboard
{
    public class ShopDashboardDTO
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public string PeriodType { get; set; } = string.Empty;

        // Livestream Statistics
        public int TotalLivestream { get; set; }
        public decimal TotalLivestreamDuration { get; set; } // in minutes
        public int TotalLivestreamViewers { get; set; }

        // Order Statistics
        public decimal TotalRevenue { get; set; }
        public int OrderInLivestream { get; set; }
        public int TotalOrder { get; set; }
        public int CompleteOrderCount { get; set; }
        public int RefundOrderCount { get; set; }
        public int ProcessingOrderCount { get; set; }
        public int CanceledOrderCount { get; set; }

        // Calculated metrics
        public decimal CompletionRate => TotalOrder > 0 ? (decimal)CompleteOrderCount / TotalOrder * 100 : 0;
        public decimal CancellationRate => TotalOrder > 0 ? (decimal)CanceledOrderCount / TotalOrder * 100 : 0;
        public decimal AverageOrderValue => TotalOrder > 0 ? TotalRevenue / TotalOrder : 0;

        // Product Statistics
        public List<TopProductDTO> TopOrderProducts { get; set; } = new List<TopProductDTO>();
        public List<TopProductDTO> TopAIRecommendedProducts { get; set; } = new List<TopProductDTO>();

        // Customer Statistics
        public int RepeatCustomerCount { get; set; }
        public int NewCustomerCount { get; set; }
        public int TotalCustomers => RepeatCustomerCount + NewCustomerCount;
        public decimal RepeatCustomerRate => TotalCustomers > 0 ? (decimal)RepeatCustomerCount / TotalCustomers * 100 : 0;

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }

    public class TopProductDTO
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ShopDashboardSummaryDTO
    {
        public Guid ShopId { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalLivestreams { get; set; }
        public int TotalCustomers { get; set; }
        public decimal CompletionRate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}