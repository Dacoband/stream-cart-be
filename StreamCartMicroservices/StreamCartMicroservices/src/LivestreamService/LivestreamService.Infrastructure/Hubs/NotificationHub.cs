using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shared.Common.Services.User;
using System;
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
            var userId = _currentUserService.GetUserId();
            if (!string.IsNullOrEmpty(userId.ToString()))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("User {UserId} connected to notification hub", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = _currentUserService.GetUserId();
            if (!string.IsNullOrEmpty(userId.ToString()))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("User {UserId} disconnected from notification hub", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}