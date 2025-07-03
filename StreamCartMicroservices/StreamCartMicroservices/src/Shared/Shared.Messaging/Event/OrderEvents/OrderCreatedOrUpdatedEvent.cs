using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messaging.Event.OrderEvents
{
    public class OrderCreatedOrUpdatedEvent
    {
        public string OrderCode { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
    }
}
