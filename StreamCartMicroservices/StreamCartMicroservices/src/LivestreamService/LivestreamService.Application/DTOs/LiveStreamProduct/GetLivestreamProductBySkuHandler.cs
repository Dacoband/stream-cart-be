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
    public class GetLivestreamProductBySkuHandler : IRequestHandler<GetLivestreamProductBySkuQuery, LivestreamProductDTO?>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetLivestreamProductBySkuHandler> _logger;

        public GetLivestreamProductBySkuHandler(
            ILivestreamRepository livestreamRepository,
            ILivestreamProductRepository livestreamProductRepository,
            IProductServiceClient productServiceClient,
            ILogger<GetLivestreamProductBySkuHandler> logger)
        {
            _livestreamRepository = livestreamRepository;
            _livestreamProductRepository = livestreamProductRepository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<LivestreamProductDTO?> Handle(GetLivestreamProductBySkuQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting product with SKU {Sku} for livestream {LivestreamId}", request.Sku, request.LivestreamId);

                // Validate input
                if (string.IsNullOrWhiteSpace(request.Sku))
                {
                    _logger.LogWarning("SKU is empty for livestream {LivestreamId}", request.LivestreamId);
                    return null;
                }

                // Kiểm tra livestream tồn tại
                var livestream = await _livestreamRepository.GetByIdAsync(request.LivestreamId.ToString());
                if (livestream == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy livestream với ID {request.LivestreamId}");
                }

                // Tìm sản phẩm theo SKU
                var livestreamProduct = await _livestreamProductRepository.GetBySkuInLivestreamAsync(request.LivestreamId, request.Sku);

                if (livestreamProduct == null)
                {
                    _logger.LogInformation("No product found with SKU {Sku} in livestream {LivestreamId}", request.Sku, request.LivestreamId);
                    return null;
                }

                // Lấy thông tin chi tiết sản phẩm từ Product Service
                var productInfo = await _productServiceClient.GetProductByIdAsync(livestreamProduct.ProductId);
                if (productInfo == null)
                {
                    _logger.LogWarning("Product {ProductId} not found in Product Service", livestreamProduct.ProductId);
                    return null;
                }

                // Lấy thông tin variant nếu có
                ProductVariantDTO? variantInfo = null;
                string variantName = "";
                string sku = "";
                int actualStock = 0;

                if (!string.IsNullOrEmpty(livestreamProduct.VariantId))
                {
                    try
                    {
                        variantInfo = await _productServiceClient.GetProductVariantAsync(livestreamProduct.ProductId, livestreamProduct.VariantId);

                        if (variantInfo != null)
                        {
                            var variantDetail = await _productServiceClient.GetCombinationStringByVariantIdAsync(Guid.Parse(livestreamProduct.VariantId));

                            variantName = !string.IsNullOrEmpty(variantDetail) ?
                                $"{productInfo.ProductName}{variantDetail}" :
                                $"{productInfo.ProductName} - Variant {livestreamProduct.VariantId}";

                            sku = variantInfo.SKU ?? "";
                            actualStock = variantInfo.Stock;
                        }
                        else
                        {
                            variantName = $"{productInfo.ProductName} - Variant {livestreamProduct.VariantId}";
                            sku = request.Sku; // Use requested SKU as fallback
                        }
                    }
                    catch (Exception variantEx)
                    {
                        _logger.LogWarning(variantEx, "Failed to get variant {VariantId} details", livestreamProduct.VariantId);
                        variantName = $"{productInfo.ProductName} - Variant {livestreamProduct.VariantId}";
                        sku = request.Sku;
                    }
                }
                else
                {
                    // Sản phẩm gốc không có variant
                    variantName = productInfo.ProductName ?? "Unknown Product";
                    actualStock = productInfo.StockQuantity;
                    sku = productInfo.SKU ?? "";
                }

                var result = new LivestreamProductDTO
                {
                    Id = livestreamProduct.Id,
                    LivestreamId = livestreamProduct.LivestreamId,
                    ProductId = livestreamProduct.ProductId,
                    VariantId = livestreamProduct.VariantId,
                    IsPin = livestreamProduct.IsPin,
                    Price = livestreamProduct.Price,
                    Stock = livestreamProduct.Stock,
                    ProductStock = actualStock,
                    CreatedAt = livestreamProduct.CreatedAt,
                    LastModifiedAt = livestreamProduct.LastModifiedAt,
                    SKU = sku,
                    ProductName = productInfo.ProductName ?? "Unknown Product",
                    ProductImageUrl = productInfo.ImageUrl ?? "",
                    VariantName = variantName
                };

                _logger.LogInformation("Successfully found product with SKU {Sku} in livestream {LivestreamId}", request.Sku, request.LivestreamId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by SKU {Sku} for livestream {LivestreamId}", request.Sku, request.LivestreamId);
                throw;
            }
        }
    }

    public class GetLivestreamProductsBySkusHandler : IRequestHandler<GetLivestreamProductsBySkusQuery, IEnumerable<LivestreamProductDTO>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetLivestreamProductsBySkusHandler> _logger;

        public GetLivestreamProductsBySkusHandler(
            IMediator mediator,
            ILogger<GetLivestreamProductsBySkusHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<IEnumerable<LivestreamProductDTO>> Handle(GetLivestreamProductsBySkusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting products with SKUs [{Skus}] for livestream {LivestreamId}",
                    string.Join(", ", request.Skus), request.LivestreamId);

                // Validate input
                if (request.Skus == null || !request.Skus.Any())
                {
                    _logger.LogWarning("No SKUs provided for livestream {LivestreamId}", request.LivestreamId);
                    return new List<LivestreamProductDTO>();
                }

                var results = new List<LivestreamProductDTO>();

                // Tìm từng sản phẩm theo SKU
                foreach (var sku in request.Skus.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    var query = new GetLivestreamProductBySkuQuery
                    {
                        LivestreamId = request.LivestreamId,
                        Sku = sku
                    };

                    var product = await _mediator.Send(query, cancellationToken);
                    if (product != null)
                    {
                        results.Add(product);
                    }
                }

                _logger.LogInformation("Successfully found {Count} products out of {Total} SKUs for livestream {LivestreamId}",
                    results.Count, request.Skus.Count(), request.LivestreamId);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by SKUs for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}