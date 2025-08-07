using System;

namespace ShopService.Application.DTOs.Dashboard
{
    public class OrderTimeSeriesDTO
    {
        public string Period { get; set; } = "daily";
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public OrderTimePoint[] DataPoints { get; set; } = Array.Empty<OrderTimePoint>();
    }

    public class OrderTimePoint
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
}