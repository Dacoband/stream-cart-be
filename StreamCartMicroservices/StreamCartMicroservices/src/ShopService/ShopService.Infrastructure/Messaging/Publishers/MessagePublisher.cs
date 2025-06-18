using MassTransit;
using ShopService.Application.Events;
using ShopService.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Infrastructure.Messaging.Publishers
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
            // Map từ Application Events sang Infrastructure Events nếu cần
            if (message is ShopApproved shopApprovedEvent)
            {
                await _publishEndpoint.Publish(new ShopApproved
                {
                    ShopId = shopApprovedEvent.ShopId,
                    ShopName = shopApprovedEvent.ShopName,
                    AccountId = shopApprovedEvent.AccountId, 
                    ApprovalDate = shopApprovedEvent.ApprovalDate
                }, cancellationToken);
                return;
            }

            // Hoặc publish message trực tiếp
            await _publishEndpoint.Publish(message, cancellationToken);
        }
    }
}