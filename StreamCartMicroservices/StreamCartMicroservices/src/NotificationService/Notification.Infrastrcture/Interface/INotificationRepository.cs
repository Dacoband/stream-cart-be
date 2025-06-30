using Notification.Domain.Entities;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Infrastrcture.Interface
{
    public interface INotificationRepository
    {
        public IQueryable<Notifications> GetByUserIdAsync(string userId);
        public Task CreateAsync(Notifications notification);
        public Task MarkAsReadAsync(Guid id);
    }
}
