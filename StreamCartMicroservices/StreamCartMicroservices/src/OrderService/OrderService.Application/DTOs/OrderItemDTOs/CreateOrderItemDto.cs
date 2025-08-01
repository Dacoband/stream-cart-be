using System;

namespace OrderService.Application.DTOs.OrderItemDTOs
{
    public class CreateOrderItemDto
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public int Quantity { get; set; }
        //public decimal UnitPrice { get; set; }
        //public string Notes { get; set; } = string.Empty;
    }
}