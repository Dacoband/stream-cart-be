using MassTransit;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Extensions;
using Shared.Messaging.Consumers;
using Shared.Messaging.Event.OrderEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Consumer
{
    public class OrderChangeComsumer : IConsumer<OrderCreatedOrUpdatedEvent>, IBaseConsumer
    {
        private readonly IProductRepository _productRepo;
        private readonly IProductVariantRepository _productVariantRepo;
        
        public OrderChangeComsumer(IProductRepository productRepo, IProductVariantRepository productVariantRepo)
        {
           _productRepo = productRepo;
            _productVariantRepo = productVariantRepo;
        }
        public async Task Consume(ConsumeContext<OrderCreatedOrUpdatedEvent> context)
        {
              if(context.Message.OrderStatus == "Waiting" || context.Message.OrderStatus == "Pending")
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var existingProduct = await _productRepo.GetByIdAsync(item.ProductId);
                    existingProduct.RemoveStock(item.Quantity);
                    await _productRepo.ReplaceAsync(item.ProductId, existingProduct);

                    if (!item.VariantId.IsNullOrEmpty()) {
                        var existingVariant = await _productVariantRepo.GetByIdAsync(item.VariantId);
                        existingVariant.DecrementStock(item.Quantity);
                        await _productVariantRepo.ReplaceAsync(item?.VariantId, existingVariant);
                    }
                   
                }
            }
            if (context.Message.OrderStatus == "Cancelled" )
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var existingProduct = await _productRepo.GetByIdAsync(item.ProductId);
                    existingProduct.AddStock(item.Quantity);
                    existingProduct.SetModifier("system");
                    await _productRepo.ReplaceAsync(item.ProductId, existingProduct);

                    if (!item.VariantId.IsNullOrEmpty())
                    {
                        var existingVariant = await _productVariantRepo.GetByIdAsync(item.VariantId);
                        existingVariant.IncrementStock(item.Quantity);
                        existingVariant.SetModifier("system");
                        await _productVariantRepo.ReplaceAsync(item?.VariantId, existingVariant);
                    }

                }
            }
        }
    }
}
