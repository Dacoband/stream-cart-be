using Microsoft.Extensions.Logging;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Interfaces;
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

        public FlashSaleJobService(
            IFlashSaleRepository flashSaleRepo,
            IProductRepository productRepo,
            IProductVariantRepository variantRepo,
            ILogger<FlashSaleJobService> logger)
        {
            _flashSaleRepo = flashSaleRepo;
            _productRepo = productRepo;
            _variantRepo = variantRepo;
            _logger = logger;
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
                    if (fs.VariantId == Guid.Empty)
                    {
                        var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());
                        if (product != null && product.DiscountPrice != fs.FlashSalePrice)
                        {
                            product.UpdatePricing(product.BasePrice,fs.FlashSalePrice);
                            await _productRepo.ReplaceAsync(product.Id.ToString(),product);
                        }
                    }
                    else
                    {
                        var variant = await _variantRepo.GetByIdAsync(fs.VariantId.ToString());
                        if (variant != null && variant.FlashSalePrice != fs.FlashSalePrice)
                        {
                            variant.UpdatePrice(variant.Price,fs.FlashSalePrice);
                            await _variantRepo.ReplaceAsync(variant.Id.ToString(), variant);
                        }
                    }
                }
                else if (isEnded)
                {
                    if (fs.VariantId == Guid.Empty)
                    {
                        var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());
                       
                            product.UpdatePricing(product.BasePrice, 0);
                            await _productRepo.ReplaceAsync(product.Id.ToString(), product);
                        
                    }
                    else
                    {
                        var variant = await _variantRepo.GetByIdAsync(fs.VariantId.ToString());
                        
                            variant.UpdatePrice(variant.Price, 0);
                            await _variantRepo.ReplaceAsync(variant.Id.ToString(), variant);
                        
                    }
                }
            }

            _logger.LogInformation("FlashSale DiscountPrice cập nhật lúc {Time}", DateTime.UtcNow);
        }
    }
}

