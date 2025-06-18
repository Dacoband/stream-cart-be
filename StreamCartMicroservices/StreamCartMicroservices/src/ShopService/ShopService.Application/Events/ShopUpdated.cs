using System;

namespace ShopService.Application.Events
{
    public class ShopUpdated
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? LogoURL { get; set; }
        public string? CoverImageURL { get; set; }
        public decimal RatingAverage { get; set; }
        public int TotalReview { get; set; }
        public int TotalProduct { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}