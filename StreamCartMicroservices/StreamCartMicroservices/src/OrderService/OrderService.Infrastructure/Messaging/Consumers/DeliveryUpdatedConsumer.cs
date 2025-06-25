using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Events;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Enums;
using Shared.Messaging.Consumers;
using System;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging.Consumers
{
    public class DeliveryUpdatedConsumer : IConsumer<DeliveryStatusUpdated>, IBaseConsumer
    {
        private readonly ILogger<DeliveryUpdatedConsumer> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IMessagePublisher _messagePublisher;

        public DeliveryUpdatedConsumer(
            ILogger<DeliveryUpdatedConsumer> logger,
            IOrderRepository orderRepository,
            IMessagePublisher messagePublisher)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _messagePublisher = messagePublisher;
        }

        public async Task Consume(ConsumeContext<DeliveryStatusUpdated> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "Delivery status updated: OrderId: {OrderId}, TrackingCode: {TrackingCode}, Status: {Status}",
                message.OrderId,
                message.TrackingCode,
                message.DeliveryStatus);

            try
            {
                // Get the order
                var order = await _orderRepository.GetByIdAsync(message.OrderId.ToString());
                if (order == null)
                {
                    _logger.LogWarning("Order not found for OrderId: {OrderId}", message.OrderId);
                    return;
                }

                // Update order based on delivery status
                var previousStatus = order.OrderStatus;
                
                switch (message.DeliveryStatus)
                {
                    case "Shipped":
                        if (order.OrderStatus == OrderStatus.Processing)
                        {
                            order.Ship(message.TrackingCode, "DeliverySystem");
                        }
                        break;
                    case "Delivered":
                        if (order.OrderStatus == OrderStatus.Shipped)
                        {
                            order.Deliver("DeliverySystem");
                        }
                        break;
                    case "Failed":
                        // Handle delivery failure if needed
                        break;
                }
                
                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                // If status changed, publish event
                if (previousStatus != order.OrderStatus)
                {
                    await _messagePublisher.PublishAsync(new OrderStatusChanged
                    {
                        OrderId = order.Id,
                        OrderCode = order.OrderCode,
                        AccountId = order.AccountId,
                        ShopId = order.ShopId,
                        PreviousStatus = previousStatus,
                        NewStatus = order.OrderStatus,
                        ChangedAt = DateTime.UtcNow,
                        ChangedBy = "DeliverySystem"
                    });
                }

                _logger.LogInformation("Successfully updated delivery status for order {OrderId}", message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery status for order {OrderId}", message.OrderId);
            }
        }
    }
}