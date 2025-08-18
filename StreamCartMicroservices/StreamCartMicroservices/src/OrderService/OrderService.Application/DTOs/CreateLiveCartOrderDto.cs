using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs
{
    public class CreateLiveCartOrderDto
    {
        [Required]
        public List<LiveCartItemDto> CartItems { get; set; } = new();

        [Required]
        public string PaymentMethod { get; set; } = "COD";

        [Required]
        public Guid DeliveryAddressId { get; set; }

        public string? CustomerNotes { get; set; }
        public string? VoucherCode { get; set; }
    }

    public class LiveCartItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        public Guid? VariantId { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; }

        [Required]
        public Guid ShopId { get; set; }

        public string? SKU { get; set; }
    }

    public class LivestreamOrderResult
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
        public Guid LivestreamId { get; set; }
    }

    public class LivestreamOrderStats
    {
        public Guid LivestreamId { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int OrdersLastHour { get; set; }
        public decimal RevenueLastHour { get; set; }
        public List<RecentOrderInfo> RecentOrders { get; set; } = new();
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class RecentOrderInfo
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
    }
}
