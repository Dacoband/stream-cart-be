using System;
using System.ComponentModel.DataAnnotations;

namespace LivestreamService.Application.DTOs.StreamEvent
{
    public class CreateStreamEventDTO
    {
        [Required]
        public Guid LivestreamId { get; set; }

        public Guid? LivestreamProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string EventType { get; set; } = string.Empty;

        [StringLength(4000)]
        public string Payload { get; set; } = string.Empty;
    }
}