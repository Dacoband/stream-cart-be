using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Infrastrcture.Settings
{
   public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = default!;
        public string DatabaseName { get; set; } = default!;
        public string NotificationCollectionName { get; set; } = "Notifications";
    }
}
