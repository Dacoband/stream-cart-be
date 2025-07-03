using MassTransit;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastrcture.Interface;
using Notification.Infrastrcture.Repositories;
using Shared.Messaging.Event.FlashSaleEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Consumer
{
    public class FlashSaleConsumer : IConsumer<FlashSaleStartEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IRealTimeNotifier _notifier;
        public FlashSaleConsumer(INotificationRepository notificationRepository, IRealTimeNotifier notifier)
        {
            _notifier = notifier;
            _notificationRepository = notificationRepository;
        }

        public async Task Consume(ConsumeContext<FlashSaleStartEvent> context)
        {
            if (context.Message.Discount == 0) return;
            var notification = new Notifications()
            {
                RecipientUserID = context.Message.UserId,
                ProductId = context.Message.ProductId,
                VariantId = context.Message.VariantId,
                Type = "FlashSale",
                Message = $"Sản phẩm {context.Message.ProductName} trong giỏ hàng của bạn đang có FlashSale",       
            };
            notification.SetCreator("system");
            await _notificationRepository.CreateAsync(notification);
            await _notifier.SendNotificationToUser(context.Message.UserId, notification);

        }
    }
}
