using OrderService.Domain.Enums;
using System;
using System.Collections.Generic;

namespace OrderService.Application.DTOs.OrderDTOs
{
    public class OrderStatisticsDto
    {
        public Guid ShopId { get; set; }
        public int TotalOrders { get; set; }
        public Dictionary<OrderStatus, int> OrdersByStatus { get; set; } = new Dictionary<OrderStatus, int>();
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommissionFees { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalItemsSold { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}