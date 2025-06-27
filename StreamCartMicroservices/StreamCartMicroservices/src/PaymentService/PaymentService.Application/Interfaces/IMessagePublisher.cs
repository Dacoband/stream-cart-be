using System.Threading;
using System.Threading.Tasks;

namespace PaymentService.Application.Interfaces
{
    /// <summary>
    /// Interface for publishing messages to message broker
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publishes a message to the message broker
        /// </summary>
        /// <typeparam name="T">Type of message</typeparam>
        /// <param name="message">Message to publish</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    }
}