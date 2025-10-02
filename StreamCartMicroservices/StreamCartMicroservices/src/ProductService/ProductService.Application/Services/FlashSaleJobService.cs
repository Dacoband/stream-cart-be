using MassTransit;
using MassTransit.Transports;
using Microsoft.Extensions.Logging;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Interfaces;
using Shared.Messaging.Event.ProductEvent;
using System;
using System.Linq;
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

        // Áp dụng sớm trước giờ bắt đầu (giây)
        private const int PreApplySeconds = 300;
        // Thời gian cho phép reset sau khi kết thúc (giây)
        private const int PostEndGraceSeconds = 30;

        public FlashSaleJobService(
            IFlashSaleRepository flashSaleRepo,
            IProductRepository productRepo,
            IProductVariantRepository variantRepo,
            ILogger<FlashSaleJobService> logger,
            IPublishEndpoint publishEndpoint)
        {
            _flashSaleRepo = flashSaleRepo;
            _productRepo = productRepo;
            _variantRepo = variantRepo;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task UpdateDiscountPricesAsync()
        {
            var nowUtc = DateTime.UtcNow;
            var processed = 0;
            var applied = 0;
            var reset = 0;
            var skipped = 0;
            var errors = 0;

            // Lấy tất cả (do repo hiện tại chưa có hàm riêng) rồi tự lọc
            var all = await _flashSaleRepo.GetAllActiveFlashSalesAsync();

            var candidates = all
                .Where(fs => !fs.IsDeleted)
                .Where(fs =>
                       // đang chạy
                       (fs.StartTime <= nowUtc && nowUtc < fs.EndTime) ||
                       // sắp chạy
                       (fs.StartTime > nowUtc && fs.StartTime <= nowUtc.AddSeconds(PreApplySeconds)) ||
                       // vừa kết thúc (để reset)
                       (fs.EndTime <= nowUtc && fs.EndTime >= nowUtc.AddSeconds(-PostEndGraceSeconds))
                )
                .OrderBy(fs => fs.StartTime)
                .ToList();

            foreach (var fs in candidates)
            {
                processed++;
                try
                {
                    bool isRunning = fs.StartTime <= nowUtc && nowUtc < fs.EndTime;
                    bool isComing = fs.StartTime > nowUtc && (fs.StartTime - nowUtc).TotalSeconds <= PreApplySeconds;
                    bool isEnded = nowUtc >= fs.EndTime;
                    bool isRecentlyEnded = isEnded && fs.EndTime >= nowUtc.AddSeconds(-PostEndGraceSeconds);
                    bool isSoldOut = fs.QuantitySold >= fs.QuantityAvailable;

                    string phase = isRunning ? "RUNNING"
                                : isComing ? "PRE_APPLY"
                                : isRecentlyEnded ? "RECENT_END"
                                : isEnded ? "ENDED"
                                : "OTHER";

                    // ÁP DỤNG
                    if ((isRunning || isComing) && !isSoldOut)
                    {
                        if (fs.VariantId == null)
                        {
                            var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());
                            if (product == null)
                            {
                                skipped++;
                                _logger.LogDebug("SKIP PRODUCT_NOT_FOUND FS={Fs} Phase={Phase}", fs.Id, phase);
                                continue;
                            }

                            if (fs.FlashSalePrice >= product.BasePrice)
                            {
                                skipped++;
                                _logger.LogWarning("SKIP INVALID_PRODUCT_PRICE FS={Fs} FlashSale={FSP} Base={Base}", fs.Id, fs.FlashSalePrice, product.BasePrice);
                                continue;
                            }

                            bool already = product.DiscountPrice.HasValue &&
                                           product.DiscountPrice.Value == fs.FlashSalePrice &&
                                           product.StartTime == fs.StartTime &&
                                           product.EndTime == fs.EndTime;

                            if (already)
                            {
                                skipped++;
                                _logger.LogDebug("SKIP PRODUCT_ALREADY_APPLIED FS={Fs}", fs.Id);
                                continue;
                            }

                            product.UpdatePricing(product.BasePrice, fs.FlashSalePrice);
                            product.StartTime = fs.StartTime;
                            product.EndTime = fs.EndTime;
                            await _productRepo.ReplaceAsync(product.Id.ToString(), product);
                            applied++;
                            _logger.LogInformation("APPLY PRODUCT FS={Fs} Price={Price} Qty={Sold}/{Avail}", fs.Id, fs.FlashSalePrice, fs.QuantitySold, fs.QuantityAvailable);

                            if (!fs.NotificationSent)
                            {
                                await PublishProductEvent(product.Id, null, fs.FlashSalePrice);
                                fs.NotificationSent = true;
                                await _flashSaleRepo.ReplaceAsync(fs.Id.ToString(), fs);
                            }
                        }
                        else
                        {
                            var variant = await _variantRepo.GetByIdAsync(fs.VariantId.ToString());
                            var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());

                            if (variant == null || product == null)
                            {
                                skipped++;
                                _logger.LogDebug("SKIP VARIANT_OR_PRODUCT_NOT_FOUND FS={Fs}", fs.Id);
                                continue;
                            }

                            if (fs.FlashSalePrice >= variant.Price)
                            {
                                skipped++;
                                _logger.LogWarning("SKIP INVALID_VARIANT_PRICE FS={Fs} FlashSale={FSP} VariantPrice={VP}", fs.Id, fs.FlashSalePrice, variant.Price);
                                continue;
                            }

                            bool already = variant.FlashSalePrice.HasValue &&
                                           variant.FlashSalePrice.Value == fs.FlashSalePrice &&
                                           product.StartTime == fs.StartTime &&
                                           product.EndTime == fs.EndTime;

                            if (already)
                            {
                                skipped++;
                                _logger.LogDebug("SKIP VARIANT_ALREADY_APPLIED FS={Fs}", fs.Id);
                                continue;
                            }

                            variant.UpdatePrice(variant.Price, fs.FlashSalePrice);
                            product.StartTime = fs.StartTime;
                            product.EndTime = fs.EndTime;
                            await _variantRepo.ReplaceAsync(variant.Id.ToString(), variant);
                            await _productRepo.ReplaceAsync(product.Id.ToString(), product);
                            applied++;
                            _logger.LogInformation("APPLY VARIANT FS={Fs} Var={Var} Price={Price} Qty={Sold}/{Avail}", fs.Id, fs.VariantId, fs.FlashSalePrice, fs.QuantitySold, fs.QuantityAvailable);

                            if (!fs.NotificationSent)
                            {
                                await PublishProductEvent(product.Id, variant.Id, fs.FlashSalePrice);
                                fs.NotificationSent = true;
                                await _flashSaleRepo.ReplaceAsync(fs.Id.ToString(), fs);
                            }
                        }
                    }
                    // RESET
                    else if ((isEnded || isSoldOut) && isRecentlyEnded)
                    {
                        if (fs.VariantId == null)
                        {
                            var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());
                            if (product != null && product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0)
                            {
                                product.UpdatePricing(product.BasePrice, 0);
                                product.StartTime = null;
                                product.EndTime = null;
                                await _productRepo.ReplaceAsync(product.Id.ToString(), product);
                                reset++;
                                _logger.LogInformation("RESET PRODUCT FS={Fs} SoldOut={SoldOut}", fs.Id, isSoldOut);
                                await PublishProductEvent(product.Id, null, product.BasePrice);
                            }
                            else
                            {
                                skipped++;
                                _logger.LogDebug("SKIP PRODUCT_NO_RESET_NEEDED FS={Fs}", fs.Id);
                            }
                        }
                        else
                        {
                            var variant = await _variantRepo.GetByIdAsync(fs.VariantId.ToString());
                            var product = await _productRepo.GetByIdAsync(fs.ProductId.ToString());
                            if (variant != null && product != null && variant.FlashSalePrice.HasValue && variant.FlashSalePrice.Value > 0)
                            {
                                variant.UpdatePrice(variant.Price, 0);
                                product.StartTime = null;
                                product.EndTime = null;
                                await _variantRepo.ReplaceAsync(variant.Id.ToString(), variant);
                                await _productRepo.ReplaceAsync(product.Id.ToString(), product);
                                reset++;
                                _logger.LogInformation("RESET VARIANT FS={Fs} Var={Var} SoldOut={SoldOut}", fs.Id, fs.VariantId, isSoldOut);
                                await PublishProductEvent(product.Id, variant.Id, variant.Price);
                            }
                            else
                            {
                                skipped++;
                                _logger.LogDebug("SKIP VARIANT_NO_RESET_NEEDED FS={Fs}", fs.Id);
                            }
                        }
                    }
                    else
                    {
                        skipped++;
                        _logger.LogDebug("SKIP OUT_OF_SCOPE FS={Fs} Phase={Phase} SoldOut={SoldOut}", fs.Id, phase, isSoldOut);
                    }
                }
                catch (Exception ex)
                {
                    errors++;
                    _logger.LogWarning(ex, "ERROR FS={Fs}", fs.Id);
                }
            }

            _logger.LogInformation("FlashSaleJob Summary processed={P} applied={A} reset={R} skipped={S} errors={E} timeUtc={Utc}",
                processed, applied, reset, skipped, errors, nowUtc);
        }

        private async Task PublishProductEvent(Guid productId, Guid? variantId, decimal price)
        {
            try
            {
                var evt = new ProductUpdatedEvent
                {
                    ProductId = productId,
                    VariantId = variantId,
                    Price = price
                };
                await _publishEndpoint.Publish(evt);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Publish event fail Product={Prod} Variant={Var}", productId, variantId);
            }
        }
    }
}