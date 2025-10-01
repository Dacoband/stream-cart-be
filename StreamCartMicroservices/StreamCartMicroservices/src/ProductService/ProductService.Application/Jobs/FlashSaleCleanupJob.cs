using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Jobs
{
    public class FlashSaleCleanupJob : BackgroundService
    {
        private readonly ILogger<FlashSaleCleanupJob> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public FlashSaleCleanupJob(
            IServiceScopeFactory scopeFactory,
            ILogger<FlashSaleCleanupJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
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

                    await ProcessEndedFlashSalesAsync(flashSaleRepo, productRepo, variantRepo, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FlashSaleCleanupJob encountered an error.");
                }

                try { await Task.Delay(_interval, stoppingToken); }
                catch (TaskCanceledException) { }
            }

            _logger.LogInformation("FlashSaleCleanupJob stopped.");
        }
        private async Task ProcessEndedFlashSalesAsync(
            IFlashSaleRepository flashSaleRepo,
            IProductRepository productRepo,
            IProductVariantRepository variantRepo,
            CancellationToken ct)
        {
            var nowUtc = DateTime.UtcNow;
            var endedFlashSales = await flashSaleRepo.FilterByAsync(fs => !fs.IsDeleted && fs.EndTime <= nowUtc);

            int processed = 0, skipped = 0;

            foreach (var fs in endedFlashSales)
            {
                if (ct.IsCancellationRequested) break;
                try
                {
                    var remaining = fs.QuantityAvailable - fs.QuantitySold;
                    if (remaining <= 0) { skipped++; continue; }

                    if (fs.VariantId.HasValue)
                    {
                        var variant = await variantRepo.GetByIdAsync(fs.VariantId.Value.ToString());
                        if (variant == null)
                        {
                            _logger.LogWarning("Variant {VariantId} not found for FlashSale {FlashSaleId}. Skipped.", fs.VariantId, fs.Id);
                            continue;
                        }

                        var releaseQty = Math.Min(remaining, variant.ReserveStock);
                        if (releaseQty <= 0) { skipped++; continue; }

                        variant.ReleaseReservedStock(releaseQty, "cron-flashsale-close");
                        await variantRepo.ReplaceAsync(variant.Id.ToString(), variant);

                        _logger.LogInformation("Released reserved stock for Variant {VariantId} +{Qty} (FS={FlashSaleId})", variant.Id, releaseQty, fs.Id);
                    }
                    else
                    {
                        var product = await productRepo.GetByIdAsync(fs.ProductId.ToString());
                        if (product == null)
                        {
                            _logger.LogWarning("Product {ProductId} not found for FlashSale {FlashSaleId}. Skipped.", fs.ProductId, fs.Id);
                            continue;
                        }

                        product.RemoveReserveStock(remaining);
                        await productRepo.ReplaceAsync(product.Id.ToString(), product);

                        _logger.LogInformation("Released reserved stock for Product {ProductId} (FS={FlashSaleId}) remaining={Remaining}", product.Id, fs.Id, remaining);
                    }

                    fs.SetModifier("cron-flashsale-close");
                    await flashSaleRepo.ReplaceAsync(fs.Id.ToString(), fs);
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