using CartService.Infrastructure.Interfaces;
using MassTransit;
using Shared.Messaging.Consumers;
using Shared.Messaging.Event.ShopEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Consumers
{
    public class ShopUpdatedConsumer : IConsumer<ShopUpdatedEvent>, IBaseConsumer
    {
        private readonly ICartItemRepository _cartItemRepository;
        public ShopUpdatedConsumer(ICartItemRepository cartItemRepository)
        {
            _cartItemRepository = cartItemRepository;
        }
        public async Task Consume(ConsumeContext<ShopUpdatedEvent> context)
        {
            var msg = context.Message;

            var cartItems = await _cartItemRepository.GetCartItemByShop(msg.ShopId);
            foreach (var cartItem in cartItems) { 
                cartItem.ShopId = msg.ShopId;
                cartItem.ShopName = msg.ShopName ?? cartItem.ShopName;
                await _cartItemRepository.ReplaceAsync(cartItem.Id.ToString(), cartItem);
            }
            return;
        }
    }
}
