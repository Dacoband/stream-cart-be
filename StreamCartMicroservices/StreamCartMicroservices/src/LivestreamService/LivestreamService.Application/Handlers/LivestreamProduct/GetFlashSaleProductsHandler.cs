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
    public class GetFlashSaleProductsHandler : IRequestHandler<GetFlashSaleProductsQuery, IEnumerable<LivestreamProductDTO>>
    {
        private readonly ILivestreamProductRepository _repository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<GetFlashSaleProductsHandler> _logger;

        public GetFlashSaleProductsHandler(
            ILivestreamProductRepository repository,
            IProductServiceClient productServiceClient,
            IShopServiceClient shopServiceClient,
            ILogger<GetFlashSaleProductsHandler> logger)
        {
            _repository = repository;
            _productServiceClient = productServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
        }

        public async Task<IEnumerable<LivestreamProductDTO>> Handle(GetFlashSaleProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var flashSaleProducts = await _repository.GetFlashSaleProductsAsync(request.LivestreamId);
                var result = new List<LivestreamProductDTO>();

                foreach (var lp in flashSaleProducts)
                {
                    try
                    {
                        var product = await _productServiceClient.GetProductByIdAsync(lp.ProductId);
                        if (product == null)
                        {
                            _logger.LogWarning("Product {ProductId} not found for livestream {LivestreamId}", lp.ProductId, request.LivestreamId);
                            continue;
                        }

                        // Optional shop info (kept as is for now)
                        var _ = await _shopServiceClient.GetShopByIdAsync(product.ShopId);

                        ProductVariantDTO? variantInfo = null;
                        string variantName = string.Empty;
                        string sku = string.Empty;
                        int actualStock = 0;

                        if (!string.IsNullOrEmpty(lp.VariantId))
                        {
                            // Variant details: SKU/Stock
                            variantInfo = await _productServiceClient.GetProductVariantAsync(lp.ProductId, lp.VariantId);

                            // Build combination name from Product Service: "Màu Đen , Model 509"
                            string? combination = null;
                            if (Guid.TryParse(lp.VariantId, out var variantGuid))
                            {
                                combination = await _productServiceClient.GetCombinationStringByVariantIdAsync(variantGuid);
                            }

                            if (!string.IsNullOrWhiteSpace(combination))
                            {
                                combination = combination.Replace(" + ", " , ");
                            }

                            variantName = !string.IsNullOrWhiteSpace(combination)
                                ? combination!
                                : (!string.IsNullOrWhiteSpace(variantInfo?.Name)
                                    ? variantInfo!.Name!
                                    : $"Variant {lp.VariantId}");

                            sku = variantInfo?.SKU ?? product.SKU ?? string.Empty;
                            actualStock = variantInfo?.Stock ?? 0;
                        }
                        else
                        {
                            // Base product (no variant)
                            variantName = product.ProductName ?? "Unknown Product";
                            sku = product.SKU ?? string.Empty;
                            actualStock = product.StockQuantity;
                        }

                        result.Add(new LivestreamProductDTO
                        {
                            Id = lp.Id,
                            LivestreamId = lp.LivestreamId,
                            ProductId = lp.ProductId,
                            VariantId = lp.VariantId,
                            //FlashSaleId = lp.FlashSaleId,
                            IsPin = lp.IsPin,
                            OriginalPrice = product.BasePrice,
                            Price = lp.Price,
                            Stock = lp.Stock,
                            ProductStock = actualStock,
                            CreatedAt = lp.CreatedAt,
                            LastModifiedAt = lp.LastModifiedAt,
                            ProductName = product.ProductName,
                            ProductImageUrl = product.PrimaryImageUrl ?? product.ImageUrl ?? string.Empty,
                            SKU = sku,
                            VariantName = variantName
                        });
                    }
                    catch (Exception itemEx)
                    {
                        _logger.LogWarning(itemEx, "Failed processing flash sale item {LivestreamProductId}", lp.Id);
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
                _logger.LogError(ex, "Error getting flash sale products for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}