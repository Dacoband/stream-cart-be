using MediatR;
using OrderService.Application.DTOs.RefundDTOs;
using System;
using System.Collections.Generic;

namespace OrderService.Application.Commands.RefundCommands
{
    public class CreateRefundRequestCommand : IRequest<RefundRequestDto>
    {
        public Guid OrderId { get; set; }
        public List<RefundItemDto> RefundItems { get; set; } = new();
        //public decimal ShippingFee { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
    }
}