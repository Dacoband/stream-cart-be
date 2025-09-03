using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Extensions;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.BackgroundServices
{
    public class OrderTrackingStatusUpdateJob : IJob
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IDeliveryApiClient _deliveryApiClient;
        private readonly ILogger<OrderTrackingStatusUpdateJob> _logger;

        // Mapping delivery status to order status
        private readonly Dictionary<string, OrderStatus> _statusMapping = new()
        {
            { "delivered", OrderStatus.Delivered },    // 4
            { "picked", OrderStatus.Shipped },             // 3  
            { "delivering", OrderStatus.OnDelivere }       // 7
        };

        public OrderTrackingStatusUpdateJob(
            IOrderRepository orderRepository,
            IDeliveryApiClient deliveryApiClient,
            ILogger<OrderTrackingStatusUpdateJob> logger)
        {
            _orderRepository = orderRepository;
            _deliveryApiClient = deliveryApiClient;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting order tracking status update job at {Time}", DateTime.UtcNow);

            try
            {
                // 1. Lấy tất cả orders có tracking code
                var ordersWithTracking = await _orderRepository.GetOrdersWithTrackingCodeAsync();

                if (!ordersWithTracking.Any())
                {
                    _logger.LogInformation("No orders with tracking codes found");
                    return;
                }

                _logger.LogInformation("Found {Count} orders with tracking codes to check", ordersWithTracking.Count());

                var updatedCount = 0;

                foreach (var order in ordersWithTracking)
                {
                    try
                    {
                        await ProcessOrderTrackingUpdate(order);
                        updatedCount++;

                        // Add small delay to avoid overwhelming the API
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing tracking update for order {OrderId} with tracking code {TrackingCode}",
                            order.Id, order.TrackingCode);
                    }
                }

                _logger.LogInformation("Order tracking status update job completed. Processed {ProcessedCount} orders, Updated {UpdatedCount} orders",
                    ordersWithTracking.Count(), updatedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in order tracking status update job");
            }
        }

        private async Task ProcessOrderTrackingUpdate(Domain.Entities.Orders order)
        {
            _logger.LogDebug("Checking delivery status for order {OrderId} with tracking code {TrackingCode}",
                order.Id, order.TrackingCode);

            // 2. Gọi API để lấy order log
            var orderLog = await _deliveryApiClient.GetOrderLogAsync(order.TrackingCode);

            if (orderLog?.Success != true || orderLog.Data?.Logs == null || !orderLog.Data.Logs.Any())
            {
                _logger.LogWarning("No delivery status found for tracking code {TrackingCode}", order.TrackingCode);
                return;
            }

            // 3. Lấy status mới nhất (log đầu tiên)
            var latestLog = orderLog.Data.Logs.OrderByDescending(l => l.UpdatedDate).First();
            var deliveryStatus = latestLog.Status?.ToLowerInvariant();

            if (string.IsNullOrEmpty(deliveryStatus))
            {
                _logger.LogWarning("Empty delivery status for tracking code {TrackingCode}", order.TrackingCode);
                return;
            }

            // 4. Kiểm tra xem có cần cập nhật không
            if (!_statusMapping.TryGetValue(deliveryStatus, out var newOrderStatus))
            {
                _logger.LogDebug("Delivery status '{DeliveryStatus}' for tracking code {TrackingCode} does not require order status update",
                    deliveryStatus, order.TrackingCode);
                return;
            }

            // 5. Kiểm tra xem order status đã đúng chưa
            if (order.OrderStatus == newOrderStatus)
            {
                _logger.LogDebug("Order {OrderId} already has correct status {Status}", order.Id, newOrderStatus);
                return;
            }

            // 6. Cập nhật order status
            _logger.LogInformation("Updating order {OrderId} from status {OldStatus} to {NewStatus} based on delivery status '{DeliveryStatus}'",
                order.Id, order.OrderStatus, newOrderStatus, deliveryStatus);

            order.UpdateStatus(newOrderStatus, "system-delivery-tracking");

            if (newOrderStatus == OrderStatus.Delivered)
            {
                order.SetActualDeliveryDate(latestLog.UpdatedDate.ToUtcSafe(), "system-delivery-tracking");
                order.PaymentStatus = PaymentStatus.Paid;
                
            }

            await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

            _logger.LogInformation("Successfully updated order {OrderId} to status {NewStatus}", order.Id, newOrderStatus);
        }
    }
}