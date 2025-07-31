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
    public class PaymentCompletedConsumer : IConsumer<PaymentCompleted>, IBaseConsumer
    {
        private readonly ILogger<PaymentCompletedConsumer> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IMessagePublisher _messagePublisher;

        /// <summary>
        /// Creates a new instance of PaymentCompletedConsumer
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="messagePublisher">Message publisher</param>
        public PaymentCompletedConsumer(
            ILogger<PaymentCompletedConsumer> logger,
            IOrderRepository orderRepository,
            IMessagePublisher messagePublisher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
        }

        /// <summary>
        /// Consumes the PaymentCompleted event
        /// </summary>
        /// <param name="context">Consumer context</param>
        public async Task Consume(ConsumeContext<PaymentCompleted> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Payment completed: OrderId: {OrderId}, TransactionId: {TransactionId}, Amount: {Amount}",
                message.OrderId,
                message.TransactionId,
                message.Amount);

            try
            {
                // Get the order
                var order = await _orderRepository.GetByIdAsync(message.OrderId.ToString());
                if (order == null)
                {
                    _logger.LogWarning("Order not found for OrderId: {OrderId}", message.OrderId);
                    return;
                }

                // Update the payment status
                order.MarkAsPaid("PaymentSystem");
                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                // Publish payment status changed event
                await _messagePublisher.PublishAsync(new PaymentStatusChanged
                {
                    OrderId = order.Id,
                    OrderCode = order.OrderCode,
                    PreviousStatus = PaymentStatus.pending,
                    NewStatus = PaymentStatus.paid,
                    ChangedAt = DateTime.UtcNow
                });

                _logger.LogInformation("Successfully processed payment for order {OrderId}", message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for OrderId: {OrderId}", message.OrderId);
            }
        }
    }
}