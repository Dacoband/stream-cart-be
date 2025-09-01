using OrderService.Domain.Enums;
using System;
using System.Collections.Generic;

namespace OrderService.Application.DTOs.RefundDTOs
{
    public class RefundRequestDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string? TrackingCode { get; set; }
        public Guid RequestedByUserId { get; set; }
        public DateTime RequestedAt { get; set; }
        public RefundStatus Status { get; set; }
        public Guid? ProcessedByUserId { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public List<RefundDetailDto> RefundDetails { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
    }

    public class RefundDetailDto
    {
        public Guid Id { get; set; }
        public Guid OrderItemId { get; set; }
        public Guid RefundRequestId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}