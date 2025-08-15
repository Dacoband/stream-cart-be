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
    public class GetLivestreamProductsHandler : IRequestHandler<GetLivestreamProductsQuery, IEnumerable<LivestreamProductDTO>>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetLivestreamProductsHandler> _logger;

        public GetLivestreamProductsHandler(
            ILivestreamRepository livestreamRepository,
            ILivestreamProductRepository livestreamProductRepository,
            IProductServiceClient productServiceClient,
            ILogger<GetLivestreamProductsHandler> logger)
        {
            _livestreamRepository = livestreamRepository;
            _livestreamProductRepository = livestreamProductRepository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<IEnumerable<LivestreamProductDTO>> Handle(GetLivestreamProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting products for livestream {LivestreamId}", request.LivestreamId);

                // Kiểm tra livestream tồn tại
                var livestream = await _livestreamRepository.GetByIdAsync(request.LivestreamId.ToString());
                if (livestream == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy livestream với ID {request.LivestreamId}");
                }

                // Lấy danh sách sản phẩm
                var products = await _livestreamProductRepository.GetByLivestreamIdAsync(request.LivestreamId);

                // Chuyển đổi sang DTO
                var result = new List<LivestreamProductDTO>();

                foreach (var product in products)
                {
                    try
                    {
                        // Lấy thông tin sản phẩm từ Product Service
                        var productInfo = await _productServiceClient.GetProductByIdAsync(product.ProductId);
                        if (productInfo == null)
                        {
                            _logger.LogWarning("Product {ProductId} not found in Product Service", product.ProductId);
                            continue;
                        }

                        // Initialize variables for variant information
                        ProductVariantDTO variantInfo = null;
                        string variantName = "";
                        string sku = ""; // Don't try to get SKU from product entity since it doesn't exist
                        int actualStock = 0;

                        // Lấy thông tin variant nếu có
                        if (!string.IsNullOrEmpty(product.VariantId))
                        {
                            try
                            {
                                variantInfo = await _productServiceClient.GetProductVariantAsync(product.ProductId, product.VariantId);

                                if (variantInfo != null)
                                {
                                    // Lấy combination string để hiển thị tên variant
                                    var variantDetail = await _productServiceClient.GetCombinationStringByVariantIdAsync(Guid.Parse(product.VariantId));

                                    variantName = variantInfo.Name ?? string.Empty;

                                    sku = variantInfo.SKU ?? "";
                                    actualStock = variantInfo.Stock; // No null coalescing needed - Stock is int
                                }
                                else
                                {
                                    variantName = $"{productInfo.ProductName} - Variant {product.VariantId}";
                                    _logger.LogWarning("Variant {VariantId} details not found for product {ProductId}",
                                        product.VariantId, product.ProductId);
                                }
                            }
                            catch (Exception variantEx)
                            {
                                _logger.LogWarning(variantEx, "Failed to get variant {VariantId} details for product {ProductId}",
                                    product.VariantId, product.ProductId);
                                variantName = $"{productInfo.ProductName} - Variant {product.VariantId}";
                            }
                        }
                        else
                        {
                            // Đây là sản phẩm gốc không có variant
                            variantName = productInfo.ProductName ?? "Unknown Product";
                            actualStock = productInfo.StockQuantity; // No null coalescing needed - StockQuantity is int
                            sku = productInfo.SKU ?? "";
                        }

                        result.Add(new LivestreamProductDTO
                        {
                            Id = product.Id,
                            LivestreamId = product.LivestreamId,
                            ProductId = product.ProductId,
                            VariantId = product.VariantId,
                            IsPin = product.IsPin,
                            Price = product.Price,
                            OriginalPrice = product.OriginalPrice,
                            Stock = product.Stock, 
                            ProductStock = actualStock, 
                            CreatedAt = product.CreatedAt,
                            LastModifiedAt = product.LastModifiedAt,
                            SKU = sku,
                            ProductName = productInfo.ProductName ?? "Unknown Product",
                            ProductImageUrl = productInfo.PrimaryImageUrl ?? productInfo.ImageUrl ?? string.Empty,
                            VariantName = variantName
                        });

                        _logger.LogDebug("Successfully processed product {ProductId} with variant {VariantId}",
                            product.ProductId, product.VariantId ?? "none");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get complete details for product {ProductId} in livestream {LivestreamId}",
                            product.ProductId, request.LivestreamId);

                        // Vẫn trả về thông tin cơ bản nếu không thể lấy thông tin chi tiết
                        result.Add(new LivestreamProductDTO
                        {
                            Id = product.Id,
                            LivestreamId = product.LivestreamId,
                            ProductId = product.ProductId,
                            VariantId = product.VariantId,
                            IsPin = product.IsPin,
                            Price = product.Price,
                            Stock = product.Stock,
                            ProductStock = 0, // Set to 0 if we can't get actual stock
                            CreatedAt = product.CreatedAt,
                            LastModifiedAt = product.LastModifiedAt,
                            SKU = "", // Empty string for fallback
                            ProductName = "Unable to load product details",
                            ProductImageUrl = "",
                            VariantName = ""
                        });
                    }
                }

                // Sort results: pinned products first, then by creation time
                var sortedResult = result
                    .OrderByDescending(p => p.IsPin)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToList();

                _logger.LogInformation("Successfully retrieved {ProductCount} products for livestream {LivestreamId}",
                    sortedResult.Count, request.LivestreamId);

                return sortedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}