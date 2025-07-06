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

                if (isRunning)
                {
                    if (fs.VariantId == null)
                    {
                        var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());
                        if (product != null)
                        {
                            product.UpdatePricing(product.BasePrice,fs.FlashSalePrice);
                            await _productRepo.ReplaceAsync(product.Id.ToString(),product);
                            var productEvent = new ProductUpdatedEvent()
                            {
                                ProductId = product.Id,
                                ProductName = product.ProductName,
                                Price = fs.FlashSalePrice,
                                Stock = product.StockQuantity,

                            };
                            try
                            {
                                await _publishEndpoint.Publish(productEvent);
                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        var variant = await _variantRepo.GetByIdAsync(fs.VariantId.ToString());
                        if (variant != null )
                        {
                            variant.UpdatePrice(variant.Price,fs.FlashSalePrice);
                            await _variantRepo.ReplaceAsync(variant.Id.ToString(), variant);
                            var productEvent = new ProductUpdatedEvent()
                            {

                                VariantId = variant.Id,
                                Price = fs.FlashSalePrice,   

                            };
                            try
                            {
                                await _publishEndpoint.Publish(productEvent);
                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }
                        }
                    }
                }
                else if (isEnded)
                {
                    
                    if (fs.VariantId == null)
                    {
                        var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());
                            if(product != null) {

                            product.UpdatePricing(product.BasePrice, 0);
                            await _productRepo.ReplaceAsync(product.Id.ToString(), product);
                            var productEvent = new ProductUpdatedEvent()
                            {
                                ProductId = product.Id,
                                ProductName = product.ProductName,
                                Price = product.BasePrice,
                                Stock = product.StockQuantity,

                            };

                            try
                            {
                                await _publishEndpoint.Publish(productEvent);
                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }

                        }

                    }
                    else
                    {
                        var variant = await _variantRepo.GetByIdAsync(fs.VariantId.ToString());
                        if (variant != null) {
                            variant.UpdatePrice(variant.Price, 0);
                            await _variantRepo.ReplaceAsync(variant.Id.ToString(), variant);
                            var productEvent = new ProductUpdatedEvent()
                            {
                                ProductId = fs.ProductId,
                                VariantId = variant.Id,
                                Price = variant.Price,

                            };
                            try
                            {
                                await _publishEndpoint.Publish(productEvent);
                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }

                        }
                            

                    }
                }
            }

            _logger.LogInformation("FlashSale DiscountPrice cập nhật lúc {Time}", DateTime.UtcNow);
        }
    }
}

