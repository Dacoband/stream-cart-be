using System;

namespace OrderService.Application.DTOs.OrderItemDTOs
{
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; }

        public Guid? VariantId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TotalPrice { get; set; }

        public string Notes { get; set; } = string.Empty;

        public Guid? RefundRequestId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
    }
}