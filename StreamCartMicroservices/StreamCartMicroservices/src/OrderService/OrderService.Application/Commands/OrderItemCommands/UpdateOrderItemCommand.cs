using System;
using MediatR;
using OrderService.Application.DTOs.OrderItemDTOs;

namespace OrderService.Application.Commands.OrderItemCommands
{
    public class UpdateOrderItemCommand : IRequest<OrderItemDto>
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string ModifiedBy { get; set; } = string.Empty;
    }
}