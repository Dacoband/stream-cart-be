using System;
using MediatR;
using OrderService.Application.DTOs.OrderItemDTOs;

namespace OrderService.Application.Commands.OrderItemCommands
{
    public class CreateOrderItemCommand : IRequest<OrderItemDto>
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}