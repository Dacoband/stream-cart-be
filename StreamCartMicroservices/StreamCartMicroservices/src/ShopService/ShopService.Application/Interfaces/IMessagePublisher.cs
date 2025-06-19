using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    }
}