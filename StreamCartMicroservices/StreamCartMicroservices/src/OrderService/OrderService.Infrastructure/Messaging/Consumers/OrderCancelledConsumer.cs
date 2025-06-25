using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Events;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using Shared.Messaging.Consumers;
using System;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging.Consumers
{
    public class OrderCancelledConsumer : IConsumer<OrderCancelRequest>, IBaseConsumer
    {
        private readonly ILogger<OrderCancelledConsumer> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IMessagePublisher _messagePublisher;

        public OrderCancelledConsumer(
            ILogger<OrderCancelledConsumer> logger,
            IOrderRepository orderRepository,
            IMessagePublisher messagePublisher)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _messagePublisher = messagePublisher;
        }

        public async Task Consume(ConsumeContext<OrderCancelRequest> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Order cancellation request: OrderId: {OrderId}, RequestedBy: {RequestedBy}",
                message.OrderId,
                message.RequestedBy);

            try
            {
                // Get the order
                var order = await _orderRepository.GetByIdAsync(message.OrderId.ToString());
                if (order == null)
                {
                    _logger.LogWarning("Order not found for OrderId: {OrderId}", message.OrderId);
                    return;
                }

                // Cancel the order if possible
                order.Cancel(message.RequestedBy);
                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                // Publish order status changed event
                await _messagePublisher.PublishAsync(new OrderStatusChanged
                {
                    OrderId = order.Id,
                    OrderCode = order.OrderCode,
                    AccountId = order.AccountId,
                    ShopId = order.ShopId,
                    PreviousStatus = message.PreviousStatus,
                    NewStatus = Domain.Enums.OrderStatus.Cancelled,
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = message.RequestedBy
                });

                _logger.LogInformation("Successfully cancelled order {OrderId}", message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", message.OrderId);
            }
        }
    }

}