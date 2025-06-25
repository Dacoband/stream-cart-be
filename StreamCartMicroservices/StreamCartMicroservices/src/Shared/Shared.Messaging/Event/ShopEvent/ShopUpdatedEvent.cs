using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messaging.Event.ShopEvent
{
    public class ShopUpdatedEvent
    {
        public Guid ShopId { get; set; }
        public string? ShopName { get; set; }
    }
}
