using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Notification.Domain.Entities;
using Notification.Infrastrcture.Data;
using Notification.Infrastrcture.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Infrastrcture.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notifications> _collection;

        public NotificationRepository(NotificationDbContext context)
        {
            _collection = context.Notifications;
        }
        public async Task CreateAsync(Notifications notification)
        {
            await _collection.InsertOneAsync(notification);
        }

        public async Task<bool> CreateNotification(Notifications notification)
        {
            try
            {
                await _collection.InsertOneAsync(notification);
                return true;
            }
            catch (Exception ex) { 
                return false;
            }
        }

        public IQueryable<Notifications> GetByUserIdAsync(string userId)
        {
            return  _collection.AsQueryable().Where(x=> x.RecipientUserID == userId);
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            var update = Builders<Notifications>.Update.Set(n => n.IsRead, true);
            await _collection.UpdateOneAsync(n => n.Id == id, update);
        }
    }
}
