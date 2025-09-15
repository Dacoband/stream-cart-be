using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Jobs
{
    public class FlashSaleCleanupJob : BackgroundService
    {
        private readonly IFlashSaleRepository _flashSaleRepo;
        private readonly IProductRepository _productRepo;
        private readonly IProductVariantRepository _variantRepo;
        private readonly ILogger<FlashSaleCleanupJob> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public FlashSaleCleanupJob(
            IFlashSaleRepository flashSaleRepo,
            IProductRepository productRepo,
            IProductVariantRepository variantRepo,
            ILogger<FlashSaleCleanupJob> logger,
            IServiceScopeFactory scopeFactory)
        {
            _flashSaleRepo = flashSaleRepo;
            _productRepo = productRepo;
            _variantRepo = variantRepo;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FlashSaleCleanupJob started.");
            try { await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); } catch { }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var flashSaleRepo = scope.ServiceProvider.GetRequiredService<IFlashSaleRepository>();
                    var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
                    var variantRepo = scope.ServiceProvider.GetRequiredService<IProductVariantRepository>();
                    await ProcessEndedFlashSalesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FlashSaleCleanupJob encountered an error.");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (TaskCanceledException) { /* shutting down */ }
            }

            _logger.LogInformation("FlashSaleCleanupJob stopped.");
        }

        private async Task ProcessEndedFlashSalesAsync(CancellationToken ct)
        {
            var nowUtc = DateTime.UtcNow;

            // Lấy tất cả FlashSale đã hết hạn và chưa bị xóa
            var endedFlashSales = await _flashSaleRepo.FilterByAsync(fs => !fs.IsDeleted && fs.EndTime <= nowUtc);

            int processed = 0, skipped = 0;
            foreach (var fs in endedFlashSales)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    var remaining = fs.QuantityAvailable - fs.QuantitySold;
                    if (remaining <= 0)
                    {
                        skipped++;
                        continue;
                    }

                    if (fs.VariantId.HasValue)
                    {
                        // Hoàn trả reserved về Variant stock
                        var variant = await _variantRepo.GetByIdAsync(fs.VariantId.Value.ToString());
                        if (variant == null)
                        {
                            _logger.LogWarning("Variant {VariantId} not found for FlashSale {FlashSaleId}. Skipped.", fs.VariantId, fs.Id);
                            continue;
                        }

                        // Clamp để tránh exception và idempotent
                        var releaseQty = Math.Min(remaining, variant.ReserveStock);
                        if (releaseQty <= 0)
                        {
                            skipped++;
                            continue;
                        }

                        variant.ReleaseReservedStock(releaseQty, "cron-flashsale-close");
                        await _variantRepo.ReplaceAsync(variant.Id.ToString(), variant);

                        _logger.LogInformation("Released reserved stock for Variant {VariantId} +{Qty} (FS={FlashSaleId})", variant.Id, releaseQty, fs.Id);
                    }
                    else
                    {
                        // Hoàn trả reserved về Product stock
                        var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());
                        if (product == null)
                        {
                            _logger.LogWarning("Product {ProductId} not found for FlashSale {FlashSaleId}. Skipped.", fs.ProductId, fs.Id);
                            continue;
                        }

                        // Product.RemoveReserveStock đã clamp sẵn theo ReserveStock
                        product.RemoveReserveStock(remaining);
                        await _productRepo.ReplaceAsync(product.Id.ToString(), product);

                        _logger.LogInformation("Released reserved stock for Product {ProductId} (FS={FlashSaleId}) remaining={Remaining}", product.Id, fs.Id, remaining);
                    }

                    // Đánh dấu đã “đóng” để idempotent lần sau:
                    // Set QuantityAvailable = QuantitySold để remaining = 0 ở lần chạy kế tiếp
                    fs.QuantityAvailable = fs.QuantitySold;
                    fs.SetModifier("cron-flashsale-close");
                    await _flashSaleRepo.ReplaceAsync(fs.Id.ToString(), fs);

                    processed++;
                }
                catch (Exception exItem)
                {
                    _logger.LogError(exItem, "Error processing FlashSale {FlashSaleId}", fs.Id);
                }
            }

            _logger.LogInformation("FlashSaleCleanupJob cycle: processed={Processed}, skipped={Skipped}, at {TimeUtc}",
                processed, skipped, nowUtc);
        }
    }
}