using MassTransit;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastrcture.Interface;
using Shared.Messaging.Consumers;
using Shared.Messaging.Event.OrderEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Consumer
{
    public class OrderChangeComsumer : IConsumer<OrderCreatedOrUpdatedEvent>, IBaseConsumer
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IRealTimeNotifier _notifier;
        private readonly IAccountServiceClient _accountServiceClient;
        public OrderChangeComsumer(INotificationRepository notificationRepository, IRealTimeNotifier notifier, IAccountServiceClient accountServiceClient)
        {
            _notifier = notifier;
            _notificationRepository = notificationRepository;
            _accountServiceClient = accountServiceClient;
        }
        public async Task Consume(ConsumeContext<OrderCreatedOrUpdatedEvent> context)
        {
            foreach(var acc in context.Message.UserId)
            {
                var account = await _accountServiceClient.GetAccountByIdAsync(Guid.Parse(acc));
                var notification = new Notifications()
                {
                    RecipientUserID = acc,
                    OrderCode = context.Message.OrderCode,
                    Type = "Order",
                    Message = $"Đơn hàng {context.Message.OrderCode} {context.Message.Message} ",
                };
                notification.SetCreator("system");
                await _notificationRepository.CreateAsync(notification);
                await _notifier.SendNotificationToUser(acc, notification);

            }


           
        }
    }
}
