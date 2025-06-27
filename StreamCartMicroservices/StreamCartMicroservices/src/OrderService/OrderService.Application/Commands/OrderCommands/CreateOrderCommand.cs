using System;
using System.Collections.Generic;
using MediatR;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.DTOs.OrderItemDTOs;

namespace OrderService.Application.Commands.OrderCommands
{
    public class CreateOrderCommand : IRequest<OrderDto>
    {
        public Guid AccountId { get; set; }
        public Guid? ShopId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public ShippingAddressDto ShippingAddress { get; set; } = new ShippingAddressDto();
        public string PaymentMethod { get; set; } = string.Empty;
        public string ShippingMethod { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public string PromoCode { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();
        public Guid? ShippingProviderId { get; set; }
        public Guid? LivestreamId { get; set; }
        public Guid? CreatedFromCommentId { get; set; }
    }
}