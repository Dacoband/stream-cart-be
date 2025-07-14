using System;

namespace LivestreamService.Application.DTOs
{
    public class ShopDTO
    {
        public Guid Id { get; set; }
        public string? ShopName { get; set; }
        public string? Description { get; set; }
        public string? LogoURL { get; set; }
        public string? CoverImageURL { get; set; }
        public decimal RatingAverage { get; set; }
        public int TotalReview { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string? ApprovalStatus { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public bool Status { get; set; }
    }
}