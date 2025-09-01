using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Extensions;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.BackgroundServices
{
    public class RefundTrackingStatusUpdateJob : IJob
    {
        private readonly IRefundRequestRepository _refundRequestRepository;
        private readonly IDeliveryApiClient _deliveryApiClient;
        private readonly ILogger<RefundTrackingStatusUpdateJob> _logger;

        private readonly Dictionary<string, RefundStatus> _statusMapping = new()
        {
            { "delivering", RefundStatus.OnDelivery },    
            { "delivered", RefundStatus.Delivered }       
        };

        public RefundTrackingStatusUpdateJob(
            IRefundRequestRepository refundRequestRepository,
            IDeliveryApiClient deliveryApiClient,
            ILogger<RefundTrackingStatusUpdateJob> logger)
        {
            _refundRequestRepository = refundRequestRepository;
            _deliveryApiClient = deliveryApiClient;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting refund tracking status update job at {Time}", DateTime.UtcNow);

            try
            {
                var refundsWithTracking = await _refundRequestRepository.GetRefundRequestsWithTrackingCodeAsync();

                var packedRefunds = refundsWithTracking.Where(r => r.Status == RefundStatus.Packed).ToList();

                if (!packedRefunds.Any())
                {
                    _logger.LogInformation("No packed refund requests with tracking codes found");
                    return;
                }

                _logger.LogInformation("Found {Count} packed refund requests with tracking codes to check", packedRefunds.Count);

                var updatedCount = 0;

                foreach (var refund in packedRefunds)
                {
                    try
                    {
                        await ProcessRefundTrackingUpdate(refund);
                        updatedCount++;

                        // Add small delay to avoid overwhelming the API
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing tracking update for refund {RefundId} with tracking code {TrackingCode}",
                            refund.Id, refund.TrackingCode);
                    }
                }

                _logger.LogInformation("Refund tracking status update job completed. Processed {ProcessedCount} refunds, Updated {UpdatedCount} refunds",
                    packedRefunds.Count, updatedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in refund tracking status update job");
            }
        }

        private async Task ProcessRefundTrackingUpdate(RefundRequest refund)
        {
            _logger.LogDebug("Checking delivery status for refund {RefundId} with tracking code {TrackingCode}",
                refund.Id, refund.TrackingCode);

          var orderLog = await _deliveryApiClient.GetOrderLogAsync(refund.TrackingCode ?? string.Empty );
            if (orderLog?.Success != true || orderLog.Data?.Logs == null || !orderLog.Data.Logs.Any())
            {
                _logger.LogWarning("No delivery status found for refund tracking code {TrackingCode}", refund.TrackingCode);
                return;
            }

            var latestLog = orderLog.Data.Logs.OrderByDescending(l => l.UpdatedDate).First();
            var deliveryStatus = latestLog.Status?.ToLowerInvariant();

            if (string.IsNullOrEmpty(deliveryStatus))
            {
                _logger.LogWarning("Empty delivery status for refund tracking code {TrackingCode}", refund.TrackingCode);
                return;
            }

            if (!_statusMapping.TryGetValue(deliveryStatus, out var newRefundStatus))
            {
                _logger.LogDebug("Delivery status '{DeliveryStatus}' for refund tracking code {TrackingCode} does not require refund status update",
                    deliveryStatus, refund.TrackingCode);
                return;
            }

            if (refund.Status == newRefundStatus)
            {
                _logger.LogDebug("Refund {RefundId} already has correct status {Status}", refund.Id, newRefundStatus);
                return;
            }

            _logger.LogInformation("Updating refund {RefundId} from status {OldStatus} to {NewStatus} based on delivery status '{DeliveryStatus}'",
                refund.Id, refund.Status, newRefundStatus, deliveryStatus);

            refund.UpdateStatus(newRefundStatus, "system-refund-tracking");

            await _refundRequestRepository.ReplaceAsync(refund.Id.ToString(), refund);

            _logger.LogInformation("Successfully updated refund {RefundId} to status {NewStatus}", refund.Id, newRefundStatus);
        }
    }
}