using System;
using System.Collections.Generic;

namespace Shared.Messaging.Event.LivestreamEvents
{
    /// <summary>
    /// Event được publish khi có đơn hàng mới trong livestream
    /// để update real-time statistics trên UI
    /// </summary>
    public class LivestreamOrderStatsUpdatedEvent
    {
        /// <summary>
        /// ID của livestream
        /// </summary>
        public Guid LivestreamId { get; set; }

        /// <summary>
        /// Số đơn hàng mới được tạo
        /// </summary>
        public int NewOrderCount { get; set; }

        /// <summary>
        /// Doanh thu mới từ các đơn hàng
        /// </summary>
        public decimal NewRevenue { get; set; }

        /// <summary>
        /// Tổng số sản phẩm mới được bán
        /// </summary>
        public int NewItemCount { get; set; }

        /// <summary>
        /// Danh sách ID các order mới
        /// </summary>
        public List<Guid> OrderIds { get; set; } = new();

        /// <summary>
        /// Thời gian tạo event
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Chi tiết sản phẩm được bán trong batch này
        /// </summary>
        public List<LivestreamProductSalesInfo> ProductsSold { get; set; } = new();
    }

    /// <summary>
    /// Thông tin chi tiết về sản phẩm được bán trong livestream
    /// </summary>
    public class LivestreamProductSalesInfo
    {
        public Guid ProductId { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
}