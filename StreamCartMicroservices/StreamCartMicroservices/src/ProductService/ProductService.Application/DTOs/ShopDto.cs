using System;

namespace ProductService.Application.DTOs
{
    public class ShopDto
    {
        public Guid Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Boolean Status { get; set; } 
        public string ApprovalStatus { get; set; } = string.Empty;
        public int TotalProduct { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}