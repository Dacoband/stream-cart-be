using System;
using MediatR;

namespace OrderService.Application.Commands.OrderItemCommands
{
    public class RemoveOrderItemCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string RemovedBy { get; set; } = string.Empty;
    }
}