using ShopService.Domain.Enums;
using System;

namespace ShopService.Application.DTOs
{
    public class ShopDto
    {
        public Guid Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoURL { get; set; } = string.Empty;
        public string CoverImageURL { get; set; } = string.Empty;
        public string BusinessLicenseImageURL { get; set; } = string.Empty;

        public decimal RatingAverage { get; set; }
        public int TotalReview { get; set; }
        
        public DateTime RegistrationDate { get; set; }
        public string ApprovalStatus { get; set; } = string.Empty;
        public DateTime? ApprovalDate { get; set; }
        
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        
        public int TotalProduct { get; set; }
        public decimal CompleteRate { get; set; }
        
        public bool Status { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public Guid AccountId { get; set; }
    }
}