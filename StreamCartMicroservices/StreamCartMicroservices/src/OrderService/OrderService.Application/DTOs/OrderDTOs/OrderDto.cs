using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Domain.Enums;
using System;
using System.Collections.Generic;

namespace OrderService.Application.DTOs.OrderDTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string CustomerNotes { get; set; } = string.Empty;
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? TimeForShop { get; set; }


        public ShippingAddressDto ShippingAddress { get; set; } = new ShippingAddressDto();

        public Guid AccountId { get; set; }
        public Guid ShopId { get; set; }

        public Guid ShippingProviderId { get; set; }

        public Guid? LivestreamId { get; set; }
        public string? VoucherCode { get; set; }

        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
}
