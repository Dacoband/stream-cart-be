using System;

namespace LivestreamService.Application.DTOs.StreamView
{
    public class StreamViewStatsDTO
    {
        public Guid LivestreamId { get; set; }
        public int TotalViews { get; set; }
        public int CurrentViewers { get; set; }
        public int UniqueViewers { get; set; }
        public TimeSpan AverageViewDuration { get; set; }
        public int PeakViewers { get; set; }
        public DateTime? PeakTime { get; set; }
    }
}