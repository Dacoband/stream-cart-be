using System;
using MediatR;
using OrderService.Application.DTOs.OrderDTOs;

namespace OrderService.Application.Commands.OrderCommands
{
    public class CancelOrderCommand : IRequest<OrderDto>
    {
        public Guid OrderId { get; set; }
        public string CancelReason { get; set; } = string.Empty;
        public string CancelledBy { get; set; } = string.Empty;
    }
}