using MassTransit;
using PaymentService.Application.Events;
using PaymentService.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Messaging.Publishers
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MessagePublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            // Map from Application Events to Infrastructure Events if needed
            if (message is PaymentProcessed paymentProcessedEvent)
            {
                await _publishEndpoint.Publish(new PaymentProcessed
                {
                    PaymentId = paymentProcessedEvent.PaymentId,
                    OrderId = paymentProcessedEvent.OrderId,
                    UserId = paymentProcessedEvent.UserId,
                    Amount = paymentProcessedEvent.Amount,
                    Status = paymentProcessedEvent.Status,
                    QrCode = paymentProcessedEvent.QrCode,
                    ProcessedAt = paymentProcessedEvent.ProcessedAt
                }, cancellationToken);
                return;
            }

            // Direct publish for other message types
            await _publishEndpoint.Publish(message, cancellationToken);
        }
    }
}