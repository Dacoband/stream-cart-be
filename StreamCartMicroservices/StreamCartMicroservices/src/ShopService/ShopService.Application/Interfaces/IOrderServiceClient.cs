using System;
using System.Threading.Tasks;
using ShopService.Application.DTOs.Dashboard;

namespace ShopService.Application.Interfaces
{
    public interface IOrderServiceClient
    {
        Task<OrderStatisticsDTO> GetOrderStatisticsAsync(Guid shopId, DateTime fromDate, DateTime toDate);
        Task<TopProductsDTO> GetTopSellingProductsAsync(Guid shopId, DateTime fromDate, DateTime toDate, int limit = 5);
        Task<CustomerStatisticsDTO> GetCustomerStatisticsAsync(Guid shopId, DateTime fromDate, DateTime toDate);
        /// <summary>
        /// Lấy dữ liệu đơn hàng theo dòng thời gian
        /// </summary>
        Task<OrderTimeSeriesDTO> GetOrderTimeSeriesAsync(Guid shopId, DateTime fromDate, DateTime toDate, string period);

        /// <summary>
        /// Lấy thống kê đơn hàng từ livestream
        /// </summary>
        Task<LivestreamOrdersDTO> GetLivestreamOrdersAsync(Guid shopId, DateTime? fromDate, DateTime? toDate);
    }

    public class OrderStatisticsDTO
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int CompleteOrderCount { get; set; }
        public int RefundOrderCount { get; set; }
        public int ProcessingOrderCount { get; set; }
        public int CanceledOrderCount { get; set; }
        public int OrdersInLivestream { get; set; }
    }

    public class TopProductsDTO
    {
        public TopProductDTO[] Products { get; set; } = Array.Empty<TopProductDTO>();
    }

    public class CustomerStatisticsDTO
    {
        public int NewCustomers { get; set; }
        public int RepeatCustomers { get; set; }
    }
}