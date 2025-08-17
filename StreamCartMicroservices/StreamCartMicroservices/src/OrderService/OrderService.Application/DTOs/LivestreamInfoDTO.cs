namespace OrderService.Application.DTOs
{
    public class LivestreamInfoDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ShopId { get; set; }
        public string? ShopName { get; set; }
        public Guid SellerId { get; set; }
        public string? SellerName { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class LivestreamBasicInfoDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid ShopId { get; set; }
        public string? ShopName { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}