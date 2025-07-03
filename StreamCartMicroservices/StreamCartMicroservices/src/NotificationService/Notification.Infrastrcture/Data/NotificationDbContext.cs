using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Notification.Infrastrcture.Settings;
using Notification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Infrastrcture.Data
{
    public class NotificationDbContext
    {
        public IMongoCollection<Notifications> Notifications { get; }

        public NotificationDbContext(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            Notifications = database.GetCollection<Notifications>(settings.Value.NotificationCollectionName);
        }
    }
}
