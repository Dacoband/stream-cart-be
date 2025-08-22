using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Enums;

namespace PaymentService.Application.Hubs
{
    public class PaymentHub : Hub
    {
        private readonly ILogger<PaymentHub> _logger;
        private readonly IPaymentService _paymentService;

        // Lưu trữ mapping giữa connectionId và thông tin order/user
        private static readonly ConcurrentDictionary<string, (Guid orderId, Guid userId, DateTime joinTime)> _orderConnections = new();

        public PaymentHub(
            ILogger<PaymentHub> logger,
            IPaymentService paymentService)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        /// <summary>
        /// Join vào room theo order ID để theo dõi trạng thái thanh toán
        /// </summary>
        /// <param name="orderId">ID của đơn hàng</param>
        public async Task JoinOrderRoom(string orderId)
        {
            try
            {
                if (!Guid.TryParse(orderId, out var orderGuid))
                {
                    await Clients.Caller.SendAsync("Error", "Invalid order ID format");
                    return;
                }

                var connectionId = Context.ConnectionId;
                var userId = GetCurrentUserId();

                if (userId == null)
                {
                    await Clients.Caller.SendAsync("Error", "Authentication required");
                    return;
                }

                // Lưu thông tin connection
                _orderConnections[connectionId] = (orderGuid, userId.Value, DateTime.UtcNow);

                // Thêm vào group theo orderId
                var groupName = $"order_{orderId}";
                await Groups.AddToGroupAsync(connectionId, groupName);

                _logger.LogInformation("User {UserId} joined order room {OrderId} via connection {ConnectionId}",
                    userId, orderId, connectionId);

                // Gửi xác nhận đã join thành công
                await Clients.Caller.SendAsync("JoinedOrderRoom", new
                {
                    OrderId = orderId,
                    Status = "Connected",
                    Message = "Successfully joined order room",
                    Timestamp = DateTime.UtcNow
                });

                // Kiểm tra trạng thái thanh toán hiện tại và gửi cho client
                await CheckAndSendCurrentPaymentStatus(orderGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining order room {OrderId}", orderId);
                await Clients.Caller.SendAsync("Error", "Failed to join order room");
            }
        }

        /// <summary>
        /// Leave khỏi room theo order ID
        /// </summary>
        /// <param name="orderId">ID của đơn hàng</param>
        public async Task LeaveOrderRoom(string orderId)
        {
            try
            {
                var connectionId = Context.ConnectionId;
                var groupName = $"order_{orderId}";

                await Groups.RemoveFromGroupAsync(connectionId, groupName);

                if (_orderConnections.TryRemove(connectionId, out var connectionInfo))
                {
                    var duration = DateTime.UtcNow - connectionInfo.joinTime;
                    _logger.LogInformation("User {UserId} left order room {OrderId} after {Duration}",
                        connectionInfo.userId, orderId, duration);
                }

                await Clients.Caller.SendAsync("LeftOrderRoom", new
                {
                    OrderId = orderId,
                    Status = "Disconnected",
                    Message = "Successfully left order room",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving order room {OrderId}", orderId);
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái thanh toán hiện tại của đơn hàng
        /// </summary>
        /// <param name="orderId">ID của đơn hàng</param>
        private async Task CheckAndSendCurrentPaymentStatus(Guid orderId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
                var payment = payments?.FirstOrDefault();

                if (payment != null)
                {
                    await Clients.Caller.SendAsync("CurrentPaymentStatus", new
                    {
                        OrderId = orderId,
                        PaymentId = payment.Id,
                        Status = payment.Status,
                        Amount = payment.Amount,
                        PaymentMethod = payment.PaymentMethod,
                        ProcessedAt = payment.ProcessedAt,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking current payment status for order {OrderId}", orderId);
            }
        }

        /// <summary>
        /// Method để gửi cập nhật trạng thái thanh toán cho tất cả clients trong order room
        /// (Được gọi từ PaymentService khi có cập nhật trạng thái)
        /// </summary>
        /// <param name="orderIds">Danh sách Order IDs</param>
        /// <param name="paymentStatus">Trạng thái thanh toán</param>
        /// <param name="paymentId">ID của payment</param>
        /// <param name="amount">Số tiền</param>
        public async Task NotifyPaymentStatusUpdate(List<Guid> orderIds, string paymentStatus, Guid paymentId, decimal amount)
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

                    await Clients.Group(groupName).SendAsync("PaymentStatusUpdated", status, ordersParam, new
                    {
                        OrderIds = orderIds,
                        PaymentId = paymentId,
                        Status = paymentStatus,
                        Amount = amount,
                        IsSuccess = isSuccess,
                        Timestamp = DateTime.UtcNow,
                        RedirectInfo = new
                        {
                            SuccessUrl = isSuccess ? $"/payment/order/results-success?orders={ordersParam}" : null,
                            FailureUrl = !isSuccess ? $"/payment/order/results-failed?orders={ordersParam}" : null
                        }
                    });
                }

                _logger.LogInformation("Successfully broadcasted payment status update to {OrderCount} order rooms", orderIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting payment status update for orders {OrderIds}", string.Join(", ", orderIds));
            }
        }

        /// <summary>
        /// Method để kiểm tra trạng thái thanh toán theo yêu cầu
        /// </summary>
        /// <param name="orderId">ID của đơn hàng</param>
        public async Task CheckPaymentStatus(string orderId)
        {
            if (!IsUserAuthenticated())
            {
                await Clients.Caller.SendAsync("Error", "Authentication required");
                return;
            }

            try
            {
                if (!Guid.TryParse(orderId, out var orderGuid))
                {
                    await Clients.Caller.SendAsync("Error", "Invalid order ID format");
                    return;
                }

                await CheckAndSendCurrentPaymentStatus(orderGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status for order {OrderId}", orderId);
                await Clients.Caller.SendAsync("Error", "Failed to check payment status");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;

            try
            {
                _logger.LogInformation("PaymentHub connection established: {ConnectionId}", connectionId);

                var isAuthenticated = Context.User?.Identity?.IsAuthenticated == true;
                var userId = GetCurrentUserId();

                await Clients.Caller.SendAsync("Connected", new
                {
                    ConnectionId = connectionId,
                    Status = "Connected",
                    IsAuthenticated = isAuthenticated,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow,
                    Message = "PaymentHub connection established successfully"
                });

                await base.OnConnectedAsync();

                _logger.LogInformation("PaymentHub handshake completed for {ConnectionId}", connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PaymentHub OnConnectedAsync for {ConnectionId}", connectionId);

                try
                {
                    await base.OnConnectedAsync();
                }
                catch (Exception baseEx)
                {
                    _logger.LogError(baseEx, "Failed to complete PaymentHub handshake for {ConnectionId}", connectionId);
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            try
            {
                // Cleanup connection tracking
                if (_orderConnections.TryRemove(connectionId, out var connectionInfo))
                {
                    var duration = DateTime.UtcNow - connectionInfo.joinTime;
                    _logger.LogInformation("PaymentHub user {UserId} disconnected from order {OrderId} after {Duration}. Connection: {ConnectionId}. Exception: {Exception}",
                        connectionInfo.userId, connectionInfo.orderId, duration, connectionId, exception?.Message ?? "None");
                }
                else
                {
                    _logger.LogInformation("PaymentHub connection {ConnectionId} disconnected. Exception: {Exception}",
                        connectionId, exception?.Message ?? "None");
                }

                await base.OnDisconnectedAsync(exception);

                _logger.LogInformation("PaymentHub disconnection cleanup completed for {ConnectionId}", connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PaymentHub OnDisconnectedAsync for {ConnectionId}", connectionId);

                try
                {
                    await base.OnDisconnectedAsync(exception);
                }
                catch (Exception baseEx)
                {
                    _logger.LogError(baseEx, "Failed to complete PaymentHub disconnection for {ConnectionId}", connectionId);
                }
            }
        }

        // Helper methods
        private bool IsUserAuthenticated()
        {
            try
            {
                return Context.User?.Identity?.IsAuthenticated == true;
            }
            catch
            {
                return false;
            }
        }

        private Guid? GetCurrentUserId()
        {
            try
            {
                var userIdString = Context.User?.FindFirst("id")?.Value
                    ?? Context.User?.FindFirst("sub")?.Value
                    ?? Context.User?.FindFirst("nameid")?.Value
                    ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return string.IsNullOrEmpty(userIdString) ? null : Guid.Parse(userIdString);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting user ID for connection {ConnectionId}", Context.ConnectionId);
                return null;
            }
        }

        private string? GetCurrentUserName()
        {
            try
            {
                return Context.User?.FindFirst("unique_name")?.Value
                       ?? Context.User?.FindFirst(ClaimTypes.Name)?.Value
                       ?? "Anonymous";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting username for connection {ConnectionId}", Context.ConnectionId);
                return "Anonymous";
            }
        }
    }
}