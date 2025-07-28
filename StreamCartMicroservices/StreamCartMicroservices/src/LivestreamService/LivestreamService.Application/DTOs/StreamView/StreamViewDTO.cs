using System;

namespace LivestreamService.Application.DTOs.StreamView
{
    public class StreamViewDTO
    {
        public Guid Id { get; set; }
        public Guid LivestreamId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Additional info
        public string? UserName { get; set; }
        public string? LivestreamTitle { get; set; }
    }
}