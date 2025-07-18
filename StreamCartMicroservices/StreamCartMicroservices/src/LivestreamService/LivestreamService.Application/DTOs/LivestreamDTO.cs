using System;

namespace LivestreamService.Application.DTOs
{
    public class LivestreamDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public Guid SellerId { get; set; }
        public string? SellerName { get; set; }
        public Guid ShopId { get; set; }
        public string? ShopName { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public bool Status { get; set; }
        public string? StreamKey { get; set; }
        public string? PlaybackUrl { get; set; }
        public string? LivekitRoomId { get; set; }
        public string? JoinToken { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int? MaxViewer { get; set; }
        public bool ApprovalStatusContent { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovalDateContent { get; set; }
        public bool IsPromoted { get; set; }
        public string? Tags { get; set; }
        public List<LivestreamProductDTO>? Products { get; set; } = new();
    }
}