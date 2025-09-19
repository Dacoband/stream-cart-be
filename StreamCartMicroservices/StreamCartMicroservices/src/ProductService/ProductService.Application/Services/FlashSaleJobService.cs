using MassTransit;
using Microsoft.Extensions.Logging;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using Shared.Messaging.Event.ProductEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class FlashSaleJobService : IFlashSaleJobService
    {
        private readonly IFlashSaleRepository _flashSaleRepo;
        private readonly IProductRepository _productRepo;
        private readonly IProductVariantRepository _variantRepo;
        private readonly ILogger<FlashSaleJobService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;


        public FlashSaleJobService(
            IFlashSaleRepository flashSaleRepo,
            IProductRepository productRepo,
            IProductVariantRepository variantRepo,
            ILogger<FlashSaleJobService> logger, IPublishEndpoint publishEndpoint)
        {
            _flashSaleRepo = flashSaleRepo;
            _productRepo = productRepo;
            _variantRepo = variantRepo;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task UpdateDiscountPricesAsync()
        {
            var now = DateTime.UtcNow;
            var flashSales = await _flashSaleRepo.GetAllActiveFlashSalesAsync();

            foreach (var fs in flashSales)
            {
                var isRunning = fs.StartTime <= now && now < fs.EndTime;
                var isEnded = now >= fs.EndTime;

                var isSoldOut = fs.QuantitySold >= fs.QuantityAvailable;

                if (isRunning && !isSoldOut)
                {
                    if (fs.VariantId == null)
                    {
                        try
                        {
                            var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());
                            if (product != null)
                            {
                                product.UpdatePricing(product.BasePrice, fs.FlashSalePrice);
                                product.StartTime = fs.StartTime;
                                product.EndTime = fs.EndTime;
                                await _productRepo.ReplaceAsync(product.Id.ToString(), product);

                                if (!fs.NotificationSent)
                                {
                                    var productEvent = new ProductUpdatedEvent()
                                    {
                                        ProductId = product.Id,
                                        ProductName = product.ProductName,
                                        Price = fs.FlashSalePrice, // giá flash sale
                                        Stock = product.StockQuantity
                                    };

                                    await _publishEndpoint.Publish(productEvent);
                                    fs.NotificationSent = true;
                                    await _flashSaleRepo.ReplaceAsync(fs.Id.ToString(), fs);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error updating product for flash sale");
                        }
                    }
                    else
                    {
                        var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());
                        var variant = await _variantRepo.GetByIdAsync(fs.VariantId.ToString());
                        if (variant != null && product != null)
                        {
                            variant.UpdatePrice(variant.Price, fs.FlashSalePrice);
                            product.StartTime = fs.StartTime;
                            product.EndTime = fs.EndTime;
                            await _productRepo.ReplaceAsync(product.Id.ToString(), product);
                            await _variantRepo.ReplaceAsync(variant.Id.ToString(), variant);

                            if (!fs.NotificationSent)
                            {
                                var productEvent = new ProductUpdatedEvent()
                                {
                                    ProductId = fs.ProductId,
                                    VariantId = variant.Id,
                                    Price = fs.FlashSalePrice
                                };

                                await _publishEndpoint.Publish(productEvent);
                                fs.NotificationSent = true;
                                await _flashSaleRepo.ReplaceAsync(fs.Id.ToString(), fs);
                            }
                        }
                    }
                }
                else if (isEnded || isSoldOut) // 🔹 NEW: nếu hết hạn hoặc bán hết thì reset giá
                {
                    if (fs.VariantId == null)
                    {
                        var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());
                        if (product != null)
                        {
                            product.UpdatePricing(product.BasePrice, 0);
                            product.StartTime = null;
                            product.EndTime = null;
                            await _productRepo.ReplaceAsync(product.Id.ToString(), product);

                            var productEvent = new ProductUpdatedEvent()
                            {
                                ProductId = product.Id,
                                ProductName = product.ProductName,
                                Price = product.BasePrice,
                                Stock = product.StockQuantity
                            };

                            await _publishEndpoint.Publish(productEvent);
                        }
                    }
                    else
                    {
                        var variant = await _variantRepo.GetByIdAsync(fs.VariantId.ToString());
                        var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());

                        if (variant != null && product != null)
                        {
                            product.StartTime = null;
                            product.EndTime = null;
                            variant.UpdatePrice(variant.Price, 0);
                            await _variantRepo.ReplaceAsync(variant.Id.ToString(), variant);
                            await _productRepo.ReplaceAsync(product.Id.ToString(), product);

                            var productEvent = new ProductUpdatedEvent()
                            {
                                ProductId = fs.ProductId,
                                VariantId = variant.Id,
                                Price = variant.Price
                            };

                            await _publishEndpoint.Publish(productEvent);
                        }
                    }

                    // 🔹 Mark flash sale as ended
                    fs.EndTime = DateTime.UtcNow; 
                    await _flashSaleRepo.ReplaceAsync(fs.Id.ToString(), fs);
                }
            }

            _logger.LogInformation("FlashSale DiscountPrice cập nhật lúc {Time}", DateTime.UtcNow);
        }
    }
}

