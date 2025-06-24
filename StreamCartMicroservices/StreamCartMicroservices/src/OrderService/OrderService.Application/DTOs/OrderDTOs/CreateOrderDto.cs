using OrderService.Application.DTOs.OrderItemDTOs;
using System;
using System.Collections.Generic;

namespace OrderService.Application.DTOs.OrderDTOs
{
    public class CreateOrderDto
    {
        public Guid AccountId { get; set; }
        public Guid ShopId { get; set; }
        public Guid ShippingProviderId { get; set; }
        public ShippingAddressDto ShippingAddress { get; set; } = new ShippingAddressDto();
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
        public string CustomerNotes { get; set; } = string.Empty;
        public Guid? LivestreamId { get; set; }
        public Guid? CreatedFromCommentId { get; set; }
    }
}