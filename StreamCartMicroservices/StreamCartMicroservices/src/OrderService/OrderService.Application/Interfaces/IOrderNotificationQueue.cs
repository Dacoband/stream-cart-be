using Shared.Messaging.Event.OrderEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces
{
    public interface IOrderNotificationQueue
    {
        void Enqueue(OrderCreatedOrUpdatedEvent item);
        ValueTask<OrderCreatedOrUpdatedEvent> DequeueAsync(CancellationToken cancellationToken);
    }
}
