using Microsoft.AspNetCore.SignalR;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;

namespace Notification.Api.Hubs
{
    public class RealtimeNotifier : IRealTimeNotifier
    {
        private readonly IHubContext<NotificationHub> _hub;

        public RealtimeNotifier(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }
        public Task SendNotificationToUser(string userId, Notifications notification)
        {
            return _hub.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
        }
    }
}
