using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging.Publishers
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<MessagePublisher> _logger;

        /// <summary>
        /// Creates a new instance of MessagePublisher
        /// </summary>
        /// <param name="publishEndpoint">MassTransit publish endpoint</param>
        /// <param name="logger">Logger</param>
        public MessagePublisher(IPublishEndpoint publishEndpoint, ILogger<MessagePublisher> logger)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Publishes a message to the message broker
        /// </summary>
        /// <typeparam name="T">Type of message</typeparam>
        /// <param name="message">Message to be published</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                _logger.LogInformation("Publishing message of type {MessageType}", typeof(T).Name);
                await _publishEndpoint.Publish(message, cancellationToken);
                _logger.LogInformation("Successfully published message of type {MessageType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message of type {MessageType}", typeof(T).Name);
                throw;
            }
        }
    }
}
