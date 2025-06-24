using System;
using MediatR;
using OrderService.Application.DTOs.OrderDTOs;

namespace OrderService.Application.Commands.OrderCommands
{
    public class UpdateTrackingCodeCommand : IRequest<OrderDto>
    {
        public Guid OrderId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string ModifiedBy { get; set; } = string.Empty;
    }
}