using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastrcture.Interface;
using Notification.Infrastrcture.Repositories;
using Shared.Messaging.Consumers;
using Shared.Messaging.Event.FlashSaleEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Consumer
{
    public class FlashSaleConsumer : IConsumer<FlashSaleStartEvent>, IBaseConsumer
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IRealTimeNotifier _notifier;
        private readonly ILogger<FlashSaleStartEvent> _logger;

        public FlashSaleConsumer(INotificationRepository notificationRepository, IRealTimeNotifier notifier, ILogger<FlashSaleStartEvent> logger)
        {
            _notifier = notifier;
            _notificationRepository = notificationRepository;
            _logger = logger;
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
            _logger.LogInformation("Flash sale creatre noti in comsumer");
            await _notifier.SendNotificationToUser(context.Message.UserId, notification);

        }
    }
}
