using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentService.Application.Services
{
    public interface IPaymentHubService
    {
        Task NotifyPaymentStatusUpdateAsync(List<Guid> orderIds, string paymentStatus, Guid paymentId, decimal amount);
    }

    public class PaymentHubService : IPaymentHubService
    {
        private readonly IHubContext<PaymentHub> _hubContext;
        private readonly ILogger<PaymentHubService> _logger;

        public PaymentHubService(
            IHubContext<PaymentHub> hubContext,
            ILogger<PaymentHubService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyPaymentStatusUpdateAsync(List<Guid> orderIds, string paymentStatus, Guid paymentId, decimal amount)
        {
            try
            {
                var isSuccess = paymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase);
                var status = isSuccess ? "success" : "failed";
                var ordersParam = string.Join(",", orderIds);

                _logger.LogInformation("Broadcasting payment status update - OrderIds: {OrderIds}, Status: {Status}, PaymentId: {PaymentId}",
                    string.Join(", ", orderIds), status, paymentId);

                // Gửi cho tất cả các order rooms liên quan
                foreach (var orderId in orderIds)
                {
                    var groupName = $"order_{orderId}";

                    await _hubContext.Clients.Group(groupName).SendAsync("PaymentStatusUpdated", status, ordersParam, new
                    {
                        OrderIds = orderIds,
                        PaymentId = paymentId,
                        Status = paymentStatus,
                        Amount = amount,
                        IsSuccess = isSuccess,
                        Timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Successfully broadcasted payment status update to {OrderCount} order rooms", orderIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting payment status update for orders {OrderIds}", string.Join(", ", orderIds));
            }
        }
    }
}