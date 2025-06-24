using System;
using System.Collections.Generic;

namespace OrderService.Application.DTOs.OrderItemDTOs
{
    public class ProductSalesStatisticsDto
    {
        public Guid ProductId { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageUnitPrice { get; set; }
        public decimal AverageDiscount { get; set; }
        public Dictionary<Guid, int> VariantQuantities { get; set; } = new Dictionary<Guid, int>();
        public int OrderCount { get; set; }
        public int RefundCount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}