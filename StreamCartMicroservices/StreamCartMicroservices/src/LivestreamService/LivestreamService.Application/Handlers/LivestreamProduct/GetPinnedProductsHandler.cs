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

namespace LivestreamService.Application.Handlers.LivestreamProductHandlers
{
    public class GetPinnedProductsHandler : IRequestHandler<GetPinnedProductsQuery, IEnumerable<LivestreamProductDTO>>
    {
        private readonly ILivestreamProductRepository _repository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetPinnedProductsHandler> _logger;

        public GetPinnedProductsHandler(
            ILivestreamProductRepository repository,
            IProductServiceClient productServiceClient,
            ILogger<GetPinnedProductsHandler> logger)
        {
            _repository = repository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<IEnumerable<LivestreamProductDTO>> Handle(GetPinnedProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var pinnedProducts = await _repository.GetPinnedProductsAsync(request.LivestreamId, request.Limit);
                var result = new List<LivestreamProductDTO>();

                foreach (var lp in pinnedProducts)
                {
                    try
                    {
                        var product = await _productServiceClient.GetProductByIdAsync(lp.ProductId);
                        if (product == null)
                        {
                            _logger.LogWarning("Product {ProductId} not found when building pinned list", lp.ProductId);
                            continue;
                        }

                        ProductVariantDTO? variantInfo = null;
                        string variantName = string.Empty;
                        string sku = string.Empty;
                        int actualStock = 0;

                        if (!string.IsNullOrEmpty(lp.VariantId))
                        {
                            // Lấy thông tin variant cơ bản
                            variantInfo = await _productServiceClient.GetProductVariantAsync(lp.ProductId, lp.VariantId);

                            // Lấy combination string "Màu Đen , Model 509"
                            string? combination = null;
                            if (Guid.TryParse(lp.VariantId, out var variantGuid))
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
                                    : $"Variant {lp.VariantId}");

                            sku = variantInfo?.SKU ?? string.Empty;
                            actualStock = variantInfo?.Stock ?? 0;
                        }
                        else
                        {
                            // Sản phẩm gốc không có variant
                            variantName ="";
                            sku = product.SKU ?? string.Empty;
                            actualStock = product.StockQuantity;
                        }

                        result.Add(new LivestreamProductDTO
                        {
                            Id = lp.Id,
                            LivestreamId = lp.LivestreamId,
                            ProductId = lp.ProductId,
                            VariantId = lp.VariantId,
                            OriginalPrice = product.BasePrice,
                            IsPin = lp.IsPin,
                            Price = lp.Price,
                            Stock = lp.Stock,
                            ProductStock = actualStock,
                            CreatedAt = lp.CreatedAt,
                            LastModifiedAt = lp.LastModifiedAt,
                            ProductName = product.ProductName ?? string.Empty,
                            ProductImageUrl = product.PrimaryImageUrl ?? string.Empty,
                            SKU = sku,
                            VariantName = variantName
                        });
                    }
                    catch (Exception exItem)
                    {
                        _logger.LogWarning(exItem, "Failed to process pinned item {LivestreamProductId}", lp.Id);

                        result.Add(new LivestreamProductDTO
                        {
                            Id = lp.Id,
                            LivestreamId = lp.LivestreamId,
                            ProductId = lp.ProductId,
                            VariantId = lp.VariantId,
                            IsPin = lp.IsPin,
                            Price = lp.Price,
                            Stock = lp.Stock,
                            ProductStock = 0,
                            CreatedAt = lp.CreatedAt,
                            LastModifiedAt = lp.LastModifiedAt,
                            ProductName = "Unavailable",
                            ProductImageUrl = "",
                            SKU = "",
                            VariantName = ""
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pinned products for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}