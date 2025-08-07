using MassTransit;

using Shared.Messaging.Consumers;
using Shared.Messaging.Event.OrderEvents;
using ShopService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Consumer
{
    public class OrderChangeComsumer : IConsumer<OrderCreatedOrUpdatedEvent>, IBaseConsumer
    {
        private readonly IShopRepository _shopRepository;
        public OrderChangeComsumer(IShopRepository shopRepository)
        {
           _shopRepository = shopRepository;
        }
        public async Task Consume(ConsumeContext<OrderCreatedOrUpdatedEvent> context)
        {
          if(context.Message.ShopRate != 0)
            {
                var existingShop = await _shopRepository.GetByIdAsync(context.Message.ShopId);
                existingShop.UpdateCompleteRate((decimal)context.Message.ShopRate);
                await _shopRepository.ReplaceAsync(context.Message.CreatedBy, existingShop);
            }  
        }
    }
}
