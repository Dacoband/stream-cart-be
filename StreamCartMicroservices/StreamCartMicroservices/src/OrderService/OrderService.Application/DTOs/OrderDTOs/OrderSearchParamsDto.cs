using OrderService.Domain.Enums;
using System;

namespace OrderService.Application.DTOs.OrderDTOs
{
    public class OrderSearchParamsDto
    {
        public Guid? AccountId { get; set; }
        public Guid? ShopId { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}