using System;
using System.Threading.Tasks;
using ShopService.Application.DTOs.Dashboard;

namespace ShopService.Application.Interfaces
{
    public interface ILivestreamServiceClient
    {
        Task<LivestreamStatisticsDTO> GetLivestreamStatisticsAsync(Guid shopId, DateTime fromDate, DateTime toDate);
    }

    public class LivestreamStatisticsDTO
    {
        public int TotalLivestreams { get; set; }
        public decimal TotalDuration { get; set; }
        public int TotalViewers { get; set; }
        public decimal AverageDuration => TotalLivestreams > 0 ? TotalDuration / TotalLivestreams : 0;
        public decimal AverageViewers => TotalLivestreams > 0 ? (decimal)TotalViewers / TotalLivestreams : 0;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}