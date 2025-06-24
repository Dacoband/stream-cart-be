using System;
using MediatR;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Domain.Enums;

namespace OrderService.Application.Commands.OrderCommands
{
    public class UpdateOrderStatusCommand : IRequest<OrderDto>
    {
        public Guid OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}