using System;
using MediatR;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Domain.Enums;

namespace OrderService.Application.Commands.OrderCommands
{
    public class UpdatePaymentStatusCommand : IRequest<OrderDto>
    {
        public Guid OrderId { get; set; }
        public PaymentStatus NewStatus { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}