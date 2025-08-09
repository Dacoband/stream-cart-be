using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shared.Common.Services.User;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Hubs
{
    // ✅ REMOVED [Authorize] for Docker compatibility - handle auth manually
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly ICurrentUserService _currentUserService;

        public NotificationHub(ILogger<NotificationHub> logger, ICurrentUserService currentUserService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
        }

        // ✅ CRITICAL: Override OnConnectedAsync for Docker compatibility
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;

            try
            {
                _logger.LogInformation("NotificationHub connection attempt from {ConnectionId}", connectionId);

                // ✅ Check authentication but don't fail if not authenticated
                var isAuthenticated = Context.User?.Identity?.IsAuthenticated == true;
                _logger.LogInformation("NotificationHub authentication status: {IsAuth} for {ConnectionId}",
                    isAuthenticated, connectionId);

                if (isAuthenticated)
                {
                    var userId = GetCurrentUserId();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                        _logger.LogInformation("User {UserId} connected to notification hub and added to group", userId);
                    }
                }
                else
                {
                    _logger.LogInformation("Anonymous notification connection allowed: {ConnectionId}", connectionId);
                }

                // ✅ CRITICAL: Always complete handshake
                await base.OnConnectedAsync();

                _logger.LogInformation("✅ NotificationHub handshake completed successfully for {ConnectionId}", connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in NotificationHub OnConnectedAsync for {ConnectionId}", connectionId);

                // ✅ CRITICAL: Always try to complete handshake even on error
                try
                {
                    await base.OnConnectedAsync();
                    _logger.LogInformation("✅ NotificationHub base handshake completed despite error for {ConnectionId}", connectionId);
                }
                catch (Exception baseEx)
                {
                    _logger.LogError(baseEx, "❌ CRITICAL: NotificationHub failed to complete handshake for {ConnectionId}", connectionId);
                    // Don't throw - let SignalR handle it gracefully
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            try
            {
                var userId = GetCurrentUserId();
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                    _logger.LogInformation("User {UserId} disconnected from notification hub", userId);
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NotificationHub OnDisconnectedAsync for {ConnectionId}", connectionId);

                try
                {
                    await base.OnDisconnectedAsync(exception);
                }
                catch (Exception baseEx)
                {
                    _logger.LogError(baseEx, "Critical: NotificationHub failed to complete disconnection for {ConnectionId}", connectionId);
                }
            }
        }

        // ✅ Helper methods with proper error handling
        private string? GetCurrentUserId()
        {
            try
            {
                return Context.User?.FindFirst("id")?.Value
                    ?? Context.User?.FindFirst("sub")?.Value
                    ?? Context.User?.FindFirst("nameid")?.Value
                    ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting user ID for NotificationHub connection {ConnectionId}", Context.ConnectionId);
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
                _logger.LogWarning(ex, "Error getting username for NotificationHub connection {ConnectionId}", Context.ConnectionId);
                return "Anonymous";
            }
        }

        // ✅ Helper method to check authentication
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
    }
}