using CartService.Infrastructure.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Consumers;
using Shared.Messaging.Event.FlashSaleEvents;
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
        private readonly IPublishEndpoint _publishEndpoint;

        public ProductUpdatedConsumer(ICartItemRepository cartItemRepository, ILogger<ProductUpdatedConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _cartItemRepository = cartItemRepository;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }
        public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
        {
            var msg = context.Message;

            var cartItems = await _cartItemRepository.GetCartByProduct(msg.ProductId, msg.VariantId);

            foreach (var item in cartItems)
            {

                item.ProductName = msg.ProductName ?? item.ProductName;
                item.PrimaryImage = msg.PrimaryImage ?? item.PrimaryImage;
                item.PriceCurrent = msg.Price ?? item.PriceCurrent;
                item.Stock = msg.Stock ?? item.Stock;
                item.Attributes = msg.Attributes ?? item.Attributes;
                item.ProductStatus = msg.ProductStatus ?? item.ProductStatus;
                await _cartItemRepository.ReplaceAsync(item.Id.ToString(), item);
                var flashSaleEvent = new FlashSaleStartEvent()
                {
                    ProductId = item.ProductId,
                    VariantId = msg.VariantId,
                    ProductName = item.ProductName,
                    UserId = item.CreatedBy,
                    Discount =msg.Price ?? 0,
                };
                try
                {
                    await _publishEndpoint.Publish(flashSaleEvent);
                    _logger.LogInformation("Publish flashsale event in flashsale; ");

                }
                catch (Exception ex) {
                
                _logger.LogInformation(ex.Message);
                }


                _logger.LogInformation("Updated CartItem: Id={CartItemId}, ProductName={ProductName}, Price={PriceSnapShot}, Stock={Stock}",
                   item.Id, item.ProductName, item.PriceSnapShot, item.Stock);
            }
            _logger.LogInformation("Finished processing ProductUpdatedEvent for ProductId={ProductId}", msg.ProductId);

            return;

        }
    }
}
