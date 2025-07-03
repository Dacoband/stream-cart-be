using PaymentService.Domain.Enums;
using System;
using System.Collections.Generic;

namespace PaymentService.Application.DTOs
{
    /// <summary>
    /// DTO for basic order information needed by payment service
    /// </summary>
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; } // Removed the invalid semicolon
    }
}