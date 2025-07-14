using MediatR;
using OrderService.Application.DTOs.OrderDTOs;
using System;
using System.Collections.Generic;

namespace OrderService.Application.Commands.OrderCommands
{
    public class CreateMultiOrderCommand : IRequest<List<OrderDto>>
    {
        public Guid AccountId { get; set; }
        public string PaymentMethod { get; set; } = "COD";
        public ShippingAddressDto ShippingAddress { get; set; } = new ShippingAddressDto();
        public Guid? LivestreamId { get; set; }
        public Guid? CreatedFromCommentId { get; set; }
        public List<CreateOrderByShopDto> OrdersByShop { get; set; } = new();
    }
}