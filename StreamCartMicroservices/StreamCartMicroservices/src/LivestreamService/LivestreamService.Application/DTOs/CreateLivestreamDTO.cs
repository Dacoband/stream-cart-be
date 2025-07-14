using System;
using System.ComponentModel.DataAnnotations;

namespace LivestreamService.Application.DTOs
{
    public class CreateLivestreamDTO
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Guid ShopId { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Tags { get; set; }
    }
}