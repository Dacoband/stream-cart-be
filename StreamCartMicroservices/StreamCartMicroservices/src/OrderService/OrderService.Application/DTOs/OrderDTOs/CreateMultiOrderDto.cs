using OrderService.Application.DTOs.OrderItemDTOs;
using System;
using System.Collections.Generic;

namespace OrderService.Application.DTOs.OrderDTOs
{
    public class CreateMultiOrderDto
    {
        public string PaymentMethod { get; set; } = "COD";
        public string AddressId { get; set; }
        public Guid? LivestreamId { get; set; }
        public Guid? CreatedFromCommentId { get; set; }
        public List<CreateOrderByShopDto> OrdersByShop { get; set; } = new();
    }

    public class CreateOrderByShopDto
    {
        public Guid ShopId { get; set; }
        public Guid ShippingProviderId { get; set; }
        public decimal ShippingFee { get; set; }
        public DateTime ExpectedDeliveryDay { get; set; }
        public string? VoucherCode { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
        public string CustomerNotes { get; set; } = string.Empty;
    }
}