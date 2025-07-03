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
        public OrderChangeComsumer(INotificationRepository notificationRepository, IRealTimeNotifier notifier)
        {
            _notifier = notifier;
            _notificationRepository = notificationRepository;
        }
        public async Task Consume(ConsumeContext<OrderCreatedOrUpdatedEvent> context)
        {
            var notification = new Notifications()
            {
                RecipientUserID = context.Message.UserId,
                OrderCode = context.Message.OrderCode,
                Type = "Order",
                Message = $"Đơn hàng {context.Message.OrderCode} {context.Message.Message} ",
            };
            notification.SetCreator("system");
            await _notificationRepository.CreateAsync(notification);
            await _notifier.SendNotificationToUser(context.Message.UserId, notification);
        }
    }
}
