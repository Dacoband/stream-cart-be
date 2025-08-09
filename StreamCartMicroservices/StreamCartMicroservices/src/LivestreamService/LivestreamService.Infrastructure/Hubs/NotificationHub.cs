using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shared.Common.Services.User;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly ICurrentUserService _currentUserService;
        public NotificationHub(ILogger<NotificationHub> logger, ICurrentUserService currentUserService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("User {UserId} connected to notification hub", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("User {UserId} disconnected from notification hub", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        private string? GetCurrentUserId()
        {
            return Context.User?.FindFirst("id")?.Value
                ?? Context.User?.FindFirst("sub")?.Value
                ?? Context.User?.FindFirst("nameid")?.Value
                ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        private string? GetCurrentUserName()
        {
            return Context.User?.FindFirst("unique_name")?.Value
             ;
        }
    }
}