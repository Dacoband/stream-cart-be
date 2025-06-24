using System;
using MediatR;
using OrderService.Application.DTOs.OrderDTOs;

namespace OrderService.Application.Commands.OrderCommands
{
    public class UpdateShippingInfoCommand : IRequest<OrderDto>
    {
        public Guid OrderId { get; set; }
        public ShippingAddressDto ShippingAddress { get; set; } = new ShippingAddressDto();
        public string ShippingMethod { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}