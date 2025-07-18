using System;

namespace LivestreamService.Application.DTOs.StreamEvent
{
    public class StreamEventDTO
    {
        public Guid Id { get; set; }
        public Guid LivestreamId { get; set; }
        public Guid UserId { get; set; }
        public Guid? LivestreamProductId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        // Additional info
        public string? UserName { get; set; }
        public string? ProductName { get; set; }
    }
}