using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces
{
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publishes a message to the message broker
        /// </summary>
        /// <typeparam name="T">Type of message</typeparam>
        /// <param name="message">Message to be published</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    }
}
