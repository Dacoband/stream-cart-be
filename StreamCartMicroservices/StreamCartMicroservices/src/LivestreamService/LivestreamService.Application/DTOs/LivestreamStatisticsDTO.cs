using System;

namespace LivestreamService.Application.DTOs
{
    /// <summary>
    /// DTO for livestream statistics
    /// </summary>
    public class LivestreamStatisticsDTO
    {
        /// <summary>
        /// Total number of livestreams in the period
        /// </summary>
        public int TotalLivestreams { get; set; }

        /// <summary>
        /// Total duration of all livestreams in minutes
        /// </summary>
        public decimal TotalDuration { get; set; }

        /// <summary>
        /// Total number of viewers across all livestreams
        /// </summary>
        public int TotalViewers { get; set; }

        /// <summary>
        /// Start date of the statistics period
        /// </summary>
        public DateTime FromDate { get; set; }

        /// <summary>
        /// End date of the statistics period
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>
        /// Average duration per livestream in minutes
        /// </summary>
        public decimal AverageDuration => TotalLivestreams > 0 ? TotalDuration / TotalLivestreams : 0;

        /// <summary>
        /// Average viewers per livestream
        /// </summary>
        public decimal AverageViewers => TotalLivestreams > 0 ? (decimal)TotalViewers / TotalLivestreams : 0;
    }
}