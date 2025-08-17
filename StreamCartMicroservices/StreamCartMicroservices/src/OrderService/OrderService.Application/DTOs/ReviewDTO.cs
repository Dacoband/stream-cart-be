using ReviewService.Domain.Enums;

namespace OrderService.Application.DTOs
{
    public class ReviewDTO
    {
        public Guid Id { get; set; }
        public Guid? OrderID { get; set; }
        public Guid? ProductID { get; set; }
        public Guid? LivestreamId { get; set; }
        public Guid AccountID { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; }
        public ReviewType Type { get; set; }
        public string TypeDisplayName => Type.ToString();
        public string? ImageUrl { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public int HelpfulCount { get; set; }
        public int UnhelpfulCount { get; set; }

        // Additional info từ các service khác
        public string? ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public string? OrderCode { get; set; }
        public string? LivestreamTitle { get; set; }
        public string? ReviewerName { get; set; }
        public string? ShopName { get; set; }
    }
}