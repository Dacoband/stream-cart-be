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

                        // Init biến cho variant
                        ProductVariantDTO? variantInfo = null;
                        string variantName = "";
                        string sku = "";
                        int actualStock = 0;

                        if (!string.IsNullOrEmpty(product.VariantId))
                        {
                            try
                            {
                                // Lấy thông tin variant cơ bản
                                variantInfo = await _productServiceClient.GetProductVariantAsync(product.ProductId, product.VariantId);

                                // Lấy combination string để build VariantName: "Màu Đen , Model 509"
                                string? combination = null;
                                if (Guid.TryParse(product.VariantId, out var variantGuid))
                                {
                                    combination = await _productServiceClient.GetCombinationStringByVariantIdAsync(variantGuid);
                                }

                                // Chuẩn hóa dấu phân cách theo yêu cầu " , "
                                if (!string.IsNullOrWhiteSpace(combination))
                                {
                                    combination = combination.Replace(" + ", " , ");
                                }

                                // Ưu tiên combination; fallback -> variantInfo.Name; cuối cùng -> "Variant {id}"
                                variantName = !string.IsNullOrWhiteSpace(combination)
                                    ? combination!
                                    : (!string.IsNullOrWhiteSpace(variantInfo?.Name)
                                        ? variantInfo!.Name!
                                        : $"Variant {product.VariantId}");

                                sku = variantInfo?.SKU ?? "";
                                actualStock = variantInfo?.Stock ?? 0;
                            }
                            catch (Exception variantEx)
                            {
                                _logger.LogWarning(variantEx, "Failed to get variant {VariantId} details for product {ProductId}",
                                    product.VariantId, product.ProductId);
                                variantName = $"Variant {product.VariantId}";
                                sku = "";
                                actualStock = 0;
                            }
                        }
                        else
                        {
                            // Sản phẩm gốc không có variant
                            variantName = "";
                            actualStock = productInfo.StockQuantity;
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

                        // Vẫn trả về thông tin cơ bản nếu lỗi
                        result.Add(new LivestreamProductDTO
                        {
                            Id = product.Id,
                            LivestreamId = product.LivestreamId,
                            ProductId = product.ProductId,
                            VariantId = product.VariantId,
                            IsPin = product.IsPin,
                            Price = product.Price,
                            Stock = product.Stock,
                            ProductStock = 0,
                            CreatedAt = product.CreatedAt,
                            LastModifiedAt = product.LastModifiedAt,
                            SKU = "",
                            ProductName = "Unable to load product details",
                            ProductImageUrl = "",
                            VariantName = ""
                        });
                    }
                }

                // Sort: ghim trước, sau đó theo CreatedAt
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