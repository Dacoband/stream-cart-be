using OrderService.Application.Interfaces;
using Shared.Messaging.Event.OrderEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Clients
{
    public class OrderNotificationQueue : IOrderNotificationQueue
    {
        private readonly Channel<OrderCreatedOrUpdatedEvent> _queue;

        public OrderNotificationQueue()
        {
            var options = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<OrderCreatedOrUpdatedEvent>(options);
        }

        public void Enqueue(OrderCreatedOrUpdatedEvent item)
        {
            if (!_queue.Writer.TryWrite(item))
                throw new InvalidOperationException("Queue is full");
        }

        public async ValueTask<OrderCreatedOrUpdatedEvent> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
