using MassTransit;
using Microsoft.Extensions.Logging;
using LivestreamService.Application.Interfaces;
using LivestreamService.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Shared.Messaging.Event.LivestreamEvents;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Consumers
{
    /// <summary>
    /// Consumer để nhận và xử lý event cập nhật thống kê đơn hàng livestream
    /// </summary>
    public class LivestreamOrderStatsUpdatedConsumer : IConsumer<LivestreamOrderStatsUpdatedEvent>
    {
        private readonly IHubContext<SignalRChatHub> _hubContext;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILogger<LivestreamOrderStatsUpdatedConsumer> _logger;

        public LivestreamOrderStatsUpdatedConsumer(
            IHubContext<SignalRChatHub> hubContext,
            ILivestreamRepository livestreamRepository,
            ILogger<LivestreamOrderStatsUpdatedConsumer> logger)
        {
            _hubContext = hubContext;
            _livestreamRepository = livestreamRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<LivestreamOrderStatsUpdatedEvent> context)
        {
            var message = context.Message;

            try
            {
                _logger.LogInformation("📊 Processing livestream order stats update for {LivestreamId}: {NewOrderCount} orders, {NewRevenue:N0}đ",
                    message.LivestreamId, message.NewOrderCount, message.NewRevenue);

                // 1. ✅ Calculate total stats (cần implement service để lấy tổng stats)
                var totalStats = await CalculateTotalStatsAsync(message.LivestreamId, message);

                // 2. ✅ REAL-TIME: Broadcast qua SignalR cho tất cả viewers
                await BroadcastLivestreamOrderStatsAsync(message.LivestreamId, totalStats);

                // 3. ✅ Broadcast thông báo đơn hàng mới cho viewers
                await BroadcastNewOrderNotificationAsync(message.LivestreamId, message);

                _logger.LogInformation("✅ Successfully processed livestream stats update for {LivestreamId}", message.LivestreamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing livestream order stats update for {LivestreamId}", message.LivestreamId);
                throw; // Re-throw để MassTransit retry nếu cần
            }
        }

        /// <summary>
        /// Broadcast order statistics update to all livestream viewers
        /// </summary>
        private async Task BroadcastLivestreamOrderStatsAsync(Guid livestreamId, LivestreamStatsData stats)
        {
            try
            {
                var groupName = $"livestream_viewers_{livestreamId}";

                // ✅ Gửi tới tất cả viewers trong livestream
                await _hubContext.Clients.Group(groupName).SendAsync("LivestreamOrderStatsUpdated", new
                {
                    LivestreamId = livestreamId,
                    NewOrderCount = stats.NewOrderCount,
                    NewRevenue = stats.NewRevenue,
                    TotalOrderCount = stats.TotalOrderCount,
                    TotalRevenue = stats.TotalRevenue,
                    AverageOrderValue = stats.AverageOrderValue,
                    OrdersLastHour = stats.OrdersLastHour,
                    RecentOrders = stats.RecentOrders,
                    TopProducts = stats.TopProducts,
                    Timestamp = stats.Timestamp,
                    Message = $"🎉 {stats.NewOrderCount} đơn hàng mới! Tổng doanh thu: {stats.TotalRevenue:N0}đ"
                });

                _logger.LogInformation("📡 Broadcasted livestream order stats to group {GroupName}: {TotalOrders} orders, {TotalRevenue:N0}đ",
                    groupName, stats.TotalOrderCount, stats.TotalRevenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error broadcasting livestream order stats for {LivestreamId}", livestreamId);
            }
        }

        /// <summary>
        /// Broadcast new order notification with celebratory effect
        /// </summary>
        private async Task BroadcastNewOrderNotificationAsync(Guid livestreamId, LivestreamOrderStatsUpdatedEvent message)
        {
            try
            {
                var groupName = $"livestream_viewers_{livestreamId}";

                // ✅ Gửi notification về đơn hàng mới với hiệu ứng
                await _hubContext.Clients.Group(groupName).SendAsync("NewLivestreamOrder", new
                {
                    LivestreamId = livestreamId,
                    OrderCount = message.NewOrderCount,
                    Revenue = message.NewRevenue,
                    ItemCount = message.NewItemCount,
                    OrderIds = message.OrderIds,
                    ProductsSold = message.ProductsSold.Select(p => new
                    {
                        ProductId = p.ProductId,
                        QuantitySold = p.QuantitySold,
                        Revenue = p.Revenue
                    }),
                    Timestamp = message.Timestamp,
                    // ✅ Celebration effect data
                    CelebrationEffect = new
                    {
                        Type = message.NewOrderCount >= 5 ? "FIREWORKS" : "CONFETTI",
                        Duration = 3000,
                        Intensity = Math.Min(message.NewOrderCount, 10)
                    },
                    Message = GenerateOrderMessage(message.NewOrderCount, message.NewRevenue),
                    ShowAnimation = true
                });

                _logger.LogInformation("🎉 Broadcasted new order notification for {LivestreamId}: {OrderCount} orders",
                    livestreamId, message.NewOrderCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error broadcasting new order notification for {LivestreamId}", livestreamId);
            }
        }

        /// <summary>
        /// Calculate total statistics for livestream (placeholder - implement with real data)
        /// </summary>
        private async Task<LivestreamStatsData> CalculateTotalStatsAsync(Guid livestreamId, LivestreamOrderStatsUpdatedEvent newData)
        {
            try
            {
                // ✅ TODO: Implement real stats calculation from database
                // For now, return mock data based on new event

                var mockTotalOrders = newData.NewOrderCount + 42; // Placeholder
                var mockTotalRevenue = newData.NewRevenue + 1500000; // Placeholder

                return new LivestreamStatsData
                {
                    LivestreamId = livestreamId,
                    NewOrderCount = newData.NewOrderCount,
                    NewRevenue = newData.NewRevenue,
                    TotalOrderCount = mockTotalOrders,
                    TotalRevenue = mockTotalRevenue,
                    AverageOrderValue = mockTotalOrders > 0 ? mockTotalRevenue / mockTotalOrders : 0,
                    OrdersLastHour = newData.NewOrderCount + 5, // Placeholder
                    RecentOrders = GenerateRecentOrders(newData.OrderIds),
                    TopProducts = GenerateTopProducts(newData.ProductsSold),
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total stats for livestream {LivestreamId}", livestreamId);

                // Fallback stats
                return new LivestreamStatsData
                {
                    LivestreamId = livestreamId,
                    NewOrderCount = newData.NewOrderCount,
                    NewRevenue = newData.NewRevenue,
                    TotalOrderCount = newData.NewOrderCount,
                    TotalRevenue = newData.NewRevenue,
                    AverageOrderValue = newData.NewRevenue,
                    OrdersLastHour = newData.NewOrderCount,
                    RecentOrders = new List<RecentOrderInfo>(),
                    TopProducts = new List<TopProductInfo>(),
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Generate order celebration message
        /// </summary>
        private string GenerateOrderMessage(int orderCount, decimal revenue)
        {
            return orderCount switch
            {
                1 => $"🎉 Có đơn hàng mới! +{revenue:N0}đ",
                >= 2 and <= 4 => $"🔥 {orderCount} đơn hàng liên tiếp! +{revenue:N0}đ",
                >= 5 and <= 9 => $"💥 BOOM! {orderCount} đơn hàng cùng lúc! +{revenue:N0}đ",
                >= 10 => $"🚀 SIÊU HOT! {orderCount} đơn hàng trong 1 lần! +{revenue:N0}đ",
                _ => $"✅ +{orderCount} đơn hàng, +{revenue:N0}đ"
            };
        }

        /// <summary>
        /// Generate recent orders data (placeholder)
        /// </summary>
        private List<RecentOrderInfo> GenerateRecentOrders(List<Guid> orderIds)
        {
            return orderIds.Take(5).Select((orderId, index) => new RecentOrderInfo
            {
                OrderId = orderId,
                OrderCode = $"LS{orderId.ToString()[..8]}",
                Amount = new Random().Next(50000, 500000),
                CustomerName = $"Khách hàng {index + 1}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-index),
                ItemCount = new Random().Next(1, 5)
            }).ToList();
        }

        /// <summary>
        /// Generate top products data (placeholder)
        /// </summary>
        private List<TopProductInfo> GenerateTopProducts(List<LivestreamProductSalesInfo> productsSold)
        {
            return productsSold.Take(3).Select(p => new TopProductInfo
            {
                ProductId = p.ProductId,
                ProductName = $"Sản phẩm {p.ProductId.ToString()[..8]}",
                QuantitySold = p.QuantitySold,
                Revenue = p.Revenue,
                TrendingScore = p.QuantitySold * 10
            }).ToList();
        }
    }

    /// <summary>
    /// DTO for livestream statistics data
    /// </summary>
    public class LivestreamStatsData
    {
        public Guid LivestreamId { get; set; }
        public int NewOrderCount { get; set; }
        public decimal NewRevenue { get; set; }
        public int TotalOrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int OrdersLastHour { get; set; }
        public List<RecentOrderInfo> RecentOrders { get; set; } = new();
        public List<TopProductInfo> TopProducts { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Recent order information
    /// </summary>
    public class RecentOrderInfo
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// Top product information
    /// </summary>
    public class TopProductInfo
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public int TrendingScore { get; set; }
    }
}