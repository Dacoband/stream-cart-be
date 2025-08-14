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
        public int CurrentCustomerViewers { get; set; }
        public int MaxCustomerViewer { get; set; } // This comes from Livestream.MaxViewer
        public Dictionary<string, int> ViewersByRole { get; set; } = new();

        // ✅ ADDITIONAL INSIGHTS
        public bool IsCurrentlyAtMaxRecord { get; set; } // True if current customer count = max record
        public DateTime? MaxViewerAchievedAt { get; set; } // When the max was achieved
    }
}