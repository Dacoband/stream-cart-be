using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Livestreamservice.Application.DTOs
{
    public class UpdateLivestreamDTO
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Tags { get; set; }
        public Guid? LivestreamHostId { get; set; }

    }
}
