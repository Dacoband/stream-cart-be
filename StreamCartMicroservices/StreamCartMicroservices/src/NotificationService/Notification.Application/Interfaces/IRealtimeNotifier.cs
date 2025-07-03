using Notification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Interfaces
{
    public interface IRealTimeNotifier
    {
        Task SendNotificationToUser(string userId, Notifications notification);
    }
}
