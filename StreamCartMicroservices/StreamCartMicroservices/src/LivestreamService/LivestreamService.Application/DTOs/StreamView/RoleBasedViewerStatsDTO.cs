using System;
using System.Collections.Generic;

namespace LivestreamService.Application.DTOs.StreamView
{
    public class RoleBasedViewerStatsDTO
    {
        public Guid LivestreamId { get; set; }
        public int TotalViewers { get; set; }
        public Dictionary<string, int> ViewersByRole { get; set; } = new Dictionary<string, int>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}