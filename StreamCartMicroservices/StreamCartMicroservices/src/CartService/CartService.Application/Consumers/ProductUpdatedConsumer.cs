using CartService.Infrastructure.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Consumers;
using Shared.Messaging.Event.ProductEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Consumers
{
    public class ProductUpdatedConsumer : IConsumer<ProductUpdatedEvent> , IBaseConsumer
    {
        private readonly ICartItemRepository _cartItemRepository;
        private readonly ILogger<ProductUpdatedConsumer> _logger;

        public ProductUpdatedConsumer(ICartItemRepository cartItemRepository, ILogger<ProductUpdatedConsumer> logger)
        {
            _cartItemRepository = cartItemRepository;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
        {
            var msg = context.Message;

            var cartItems = await _cartItemRepository.GetCartByProduct(msg.ProductId, msg.VariantId);

            foreach (var item in cartItems)
            {

                item.ProductName = msg.ProductName ?? item.ProductName;
                item.PrimaryImage = msg.PrimaryImage ?? item.PrimaryImage;
                item.PriceSnapShot = msg.Price ?? item.PriceCurrent;
                item.Stock = msg.Stock ?? item.Stock;
                item.Attributes = msg.Attributes ?? item.Attributes;
                item.ProductStatus = msg.ProductStatus ?? item.ProductStatus;
                await _cartItemRepository.ReplaceAsync(item.Id.ToString(), item);
                _logger.LogInformation("Updated CartItem: Id={CartItemId}, ProductName={ProductName}, Price={PriceSnapShot}, Stock={Stock}",
                   item.Id, item.ProductName, item.PriceSnapShot, item.Stock);
            }
            _logger.LogInformation("Finished processing ProductUpdatedEvent for ProductId={ProductId}", msg.ProductId);

            return;

        }
    }
}
