using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities
{
    public class ShopDashboard : BaseEntity
    {
        public Guid ShopId { get; private set; }
        public DateTime FromTime { get; private set; }
        public DateTime ToTime { get; private set; }
        public string PeriodType { get; private set; } // Daily, Weekly, Monthly, Yearly

        // Livestream Statistics
        public int TotalLivestream { get; private set; }
        public decimal TotalLivestreamDuration { get; private set; } // in minutes
        public int TotalLivestreamViewers { get; private set; }

        // Order Statistics
        public decimal TotalRevenue { get; private set; }
        public int OrderInLivestream { get; private set; }
        public int TotalOrder { get; private set; }
        public int CompleteOrderCount { get; private set; }
        public int RefundOrderCount { get; private set; }
        public int ProcessingOrderCount { get; private set; }
        public int CanceledOrderCount { get; private set; }

        // Product Statistics
        public List<TopProductInfo> TopOrderProducts { get; private set; } = new List<TopProductInfo>();
        public List<TopProductInfo> TopAIRecommendedProducts { get; private set; } = new List<TopProductInfo>();

        // Customer Statistics
        public int RepeatCustomerCount { get; private set; }
        public int NewCustomerCount { get; private set; }

        public string? Notes { get; private set; }

        protected ShopDashboard() : base() { }

        public ShopDashboard(
            Guid shopId,
            DateTime fromTime,
            DateTime toTime,
            string periodType) : base()
        {
            ShopId = shopId;
            FromTime = fromTime;
            ToTime = toTime;
            PeriodType = periodType;
        }

        public void UpdateOrderStatistics(
            decimal totalRevenue,
            int totalOrder,
            int orderInLivestream,
            int completeOrderCount,
            int refundOrderCount,
            int processingOrderCount,
            int canceledOrderCount)
        {
            TotalRevenue = totalRevenue;
            TotalOrder = totalOrder;
            OrderInLivestream = orderInLivestream;
            CompleteOrderCount = completeOrderCount;
            RefundOrderCount = refundOrderCount;
            ProcessingOrderCount = processingOrderCount;
            CanceledOrderCount = canceledOrderCount;
            SetModifier(null);
        }

        public void UpdateLivestreamStatistics(
            int totalLivestream,
            decimal totalLivestreamDuration,
            int totalLivestreamViewers)
        {
            TotalLivestream = totalLivestream;
            TotalLivestreamDuration = totalLivestreamDuration;
            TotalLivestreamViewers = totalLivestreamViewers;
            SetModifier(null);
        }

        public void UpdateCustomerStatistics(
            int repeatCustomerCount,
            int newCustomerCount)
        {
            RepeatCustomerCount = repeatCustomerCount;
            NewCustomerCount = newCustomerCount;
            SetModifier(null);
        }

        public void SetTopOrderProducts(List<TopProductInfo> topProducts)
        {
            TopOrderProducts = topProducts;
            SetModifier(null);
        }

        public void SetTopAIProducts(List<TopProductInfo> topAIProducts)
        {
            TopAIRecommendedProducts = topAIProducts;
            SetModifier(null);
        }

        public void SetNotes(string? notes)
        {
            Notes = notes;
            SetModifier(null);
        }

        public override bool IsValid()
        {
            return ShopId != Guid.Empty && ToTime > FromTime;
        }
    }

    public class TopProductInfo
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal Revenue { get; set; }
    }
}