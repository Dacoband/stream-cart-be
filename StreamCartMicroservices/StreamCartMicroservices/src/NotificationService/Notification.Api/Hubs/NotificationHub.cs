using Microsoft.AspNetCore.SignalR;
using Notification.Domain.Entities;

namespace Notification.Api.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotificationToUser(string userId, Notifications notification)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }
    }
}
