using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.LivestreamProduct
{
    /// <summary>
    /// Handler for getting a specific product in livestream with all its variants information
    /// </summary>
    public class GetProductLiveStreamHandler : IRequestHandler<GetProductLiveStreamQuery, ProductLiveStreamDTO>
    {
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetProductLiveStreamHandler> _logger;

        public GetProductLiveStreamHandler(
            ILivestreamProductRepository livestreamProductRepository,
            IProductServiceClient productServiceClient,
            ILogger<GetProductLiveStreamHandler> logger)
        {
            _livestreamProductRepository = livestreamProductRepository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<ProductLiveStreamDTO> Handle(GetProductLiveStreamQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting product {ProductId} details for livestream {LivestreamId}",
                    request.ProductId, request.LivestreamId);

                // Get all variants of this product in the livestream
                var livestreamProducts = await _livestreamProductRepository.GetByLivestreamIdAsync(request.LivestreamId);
                var productVariantsInLivestream = livestreamProducts
                    .Where(lp => lp.ProductId == request.ProductId)
                    .ToList();

                if (!productVariantsInLivestream.Any())
                {
                    _logger.LogWarning("Product {ProductId} not found in livestream {LivestreamId}",
                        request.ProductId, request.LivestreamId);
                    return null;
                }

                // Get product information from Product Service
                var productInfo = await _productServiceClient.GetProductByIdAsync(request.ProductId);
                if (productInfo == null)
                {
                    _logger.LogWarning("Product {ProductId} not found in Product Service", request.ProductId);
                    throw new KeyNotFoundException($"Product {request.ProductId} not found");
                }

                var result = new ProductLiveStreamDTO
                {
                    ProductId = request.ProductId,
                    ProductName = productInfo.ProductName ?? "Unknown Product",
                    ProductImageUrl = productInfo.PrimaryImageUrl ?? productInfo.ImageUrl ?? "",
                    HasVariants = productVariantsInLivestream.Count > 1 || !string.IsNullOrEmpty(productVariantsInLivestream.First().VariantId),
                    Variants = new List<LivestreamProductVariantDTO>()
                };

                int totalActualStock = 0;
                DateTime? latestModified = null;
                DateTime earliestCreated = DateTime.MaxValue;

                // Process each variant in the livestream
                foreach (var livestreamProduct in productVariantsInLivestream)
                {
                    try
                    {
                        string variantName = "";
                        string sku = "";
                        int actualStock = 0;

                        // Get variant details if this is a variant
                        if (!string.IsNullOrEmpty(livestreamProduct.VariantId))
                        {
                            var variantInfo = await _productServiceClient.GetProductVariantAsync(
                                livestreamProduct.ProductId, livestreamProduct.VariantId);

                            // Lấy combination string để hiển thị tên variant: "Màu Đen , Model 509"
                            string? combination = null;
                            if (Guid.TryParse(livestreamProduct.VariantId, out var variantGuid))
                            {
                                combination = await _productServiceClient.GetCombinationStringByVariantIdAsync(variantGuid);
                            }

                            if (!string.IsNullOrWhiteSpace(combination))
                            {
                                // Chuẩn hóa dấu phân cách theo yêu cầu
                                combination = combination.Replace(" + ", " , ");
                            }

                            variantName = !string.IsNullOrWhiteSpace(combination)
                                ? combination!
                                : (!string.IsNullOrWhiteSpace(variantInfo?.Name)
                                    ? variantInfo!.Name!
                                    : $"Variant {livestreamProduct.VariantId}");

                            sku = variantInfo?.SKU ?? "";
                            actualStock = variantInfo?.Stock ?? 0;
                        }
                        else
                        {
                            // This is the base product without variants
                            variantName = "";
                            actualStock = productInfo.StockQuantity;
                            sku = productInfo.SKU ?? "";
                        }

                        result.Variants.Add(new LivestreamProductVariantDTO
                        {
                            Id = livestreamProduct.Id,
                            LivestreamId = livestreamProduct.LivestreamId,
                            ProductId = livestreamProduct.ProductId,
                            VariantId = livestreamProduct.VariantId ?? "",
                            VariantName = variantName,
                            SKU = sku,
                            IsPin = livestreamProduct.IsPin,
                            Price = livestreamProduct.Price,
                            LivestreamStock = livestreamProduct.Stock,
                            ActualStock = actualStock,
                            CreatedAt = livestreamProduct.CreatedAt,
                            LastModifiedAt = livestreamProduct.LastModifiedAt
                        });

                        totalActualStock += actualStock;

                        // Track earliest creation and latest modification
                        if (livestreamProduct.CreatedAt < earliestCreated)
                            earliestCreated = livestreamProduct.CreatedAt;

                        if (livestreamProduct.LastModifiedAt.HasValue &&
                            (!latestModified.HasValue || livestreamProduct.LastModifiedAt > latestModified))
                            latestModified = livestreamProduct.LastModifiedAt;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get details for variant {VariantId} of product {ProductId}",
                            livestreamProduct.VariantId, request.ProductId);

                        // Add variant with basic info even if details fetch failed
                        result.Variants.Add(new LivestreamProductVariantDTO
                        {
                            Id = livestreamProduct.Id,
                            LivestreamId = livestreamProduct.LivestreamId,
                            ProductId = livestreamProduct.ProductId,
                            VariantId = livestreamProduct.VariantId ?? "",
                            VariantName = $"{productInfo.ProductName} - Unknown Variant",
                            SKU = "",
                            IsPin = livestreamProduct.IsPin,
                            Price = livestreamProduct.Price,
                            LivestreamStock = livestreamProduct.Stock,
                            ActualStock = 0,
                            CreatedAt = livestreamProduct.CreatedAt,
                            LastModifiedAt = livestreamProduct.LastModifiedAt
                        });
                    }
                }

                result.TotalActualStock = totalActualStock;
                result.CreatedAt = earliestCreated != DateTime.MaxValue ? earliestCreated : DateTime.UtcNow;
                result.LastModifiedAt = latestModified;

                // Sort variants: pinned first, then by creation time
                result.Variants = result.Variants
                    .OrderByDescending(v => v.IsPin)
                    .ThenBy(v => v.CreatedAt)
                    .ToList();

                _logger.LogInformation("Successfully retrieved product {ProductId} with {VariantCount} variants for livestream {LivestreamId}",
                    request.ProductId, result.Variants.Count, request.LivestreamId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId} details for livestream {LivestreamId}",
                    request.ProductId, request.LivestreamId);
                throw;
            }
        }
    }
}