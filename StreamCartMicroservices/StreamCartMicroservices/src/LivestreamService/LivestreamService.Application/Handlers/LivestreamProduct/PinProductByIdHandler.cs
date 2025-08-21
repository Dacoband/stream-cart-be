using LivestreamService.Application.Commands.LiveStreamService;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.LivestreamProduct
{
    public class PinProductByIdHandler : IRequestHandler<PinProductByIdCommand, LivestreamProductDTO>
    {
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<PinProductByIdHandler> _logger;

        public PinProductByIdHandler(
            ILivestreamProductRepository livestreamProductRepository,
            ILivestreamRepository livestreamRepository,
            IProductServiceClient productServiceClient,
            ILogger<PinProductByIdHandler> logger)
        {
            _livestreamProductRepository = livestreamProductRepository;
            _livestreamRepository = livestreamRepository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<LivestreamProductDTO> Handle(PinProductByIdCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var livestreamProduct = await _livestreamProductRepository.GetByIdAsync(request.Id.ToString());
                if (livestreamProduct == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID {request.Id}");
                }

                // Verify livestream exists
                var livestream = await _livestreamRepository.GetByIdAsync(livestreamProduct.LivestreamId.ToString());
                if (livestream == null)
                {
                    throw new KeyNotFoundException("Livestream not found");
                }

                // Handle pinning logic: ensure only one product is pinned
                if (request.IsPin)
                {
                    var currentPinnedProduct = await _livestreamProductRepository.GetCurrentPinnedProductAsync(livestreamProduct.LivestreamId);
                    if (currentPinnedProduct != null && currentPinnedProduct.Id != livestreamProduct.Id)
                    {
                        currentPinnedProduct.SetPin(false, request.SellerId.ToString());
                        await _livestreamProductRepository.ReplaceAsync(currentPinnedProduct.Id.ToString(), currentPinnedProduct);
                    }
                }

                // Update pin status
                livestreamProduct.SetPin(request.IsPin, request.SellerId.ToString());
                await _livestreamProductRepository.ReplaceAsync(livestreamProduct.Id.ToString(), livestreamProduct);

                // Build response details from Product Service
                var product = await _productServiceClient.GetProductByIdAsync(livestreamProduct.ProductId);

                ProductVariantDTO? variantInfo = null;
                string variantName = string.Empty;
                string sku = string.Empty;
                int productStock = 0;

                if (!string.IsNullOrEmpty(livestreamProduct.VariantId))
                {
                    // Get variant basic info (SKU, stock)
                    variantInfo = await _productServiceClient.GetProductVariantAsync(
                        livestreamProduct.ProductId, livestreamProduct.VariantId);

                    // Get combination string (e.g. "Màu Đen , Model 509")
                    string? combination = null;
                    if (Guid.TryParse(livestreamProduct.VariantId, out var variantGuid))
                    {
                        combination = await _productServiceClient.GetCombinationStringByVariantIdAsync(variantGuid);
                    }

                    if (!string.IsNullOrWhiteSpace(combination))
                    {
                        // Normalize delimiter to " , " per requirement
                        combination = combination.Replace(" + ", " , ");
                    }

                    variantName = !string.IsNullOrWhiteSpace(combination)
                        ? combination!
                        : (!string.IsNullOrWhiteSpace(variantInfo?.Name)
                            ? variantInfo!.Name!
                            : $"Variant {livestreamProduct.VariantId}");

                    sku = variantInfo?.SKU ?? product?.SKU ?? string.Empty;
                    productStock = variantInfo?.Stock ?? 0;
                }
                else
                {
                    // Base product without variant
                    variantName = product?.ProductName ?? "Unknown Product";
                    sku = product?.SKU ?? string.Empty;
                    productStock = product?.StockQuantity ?? 0;
                }

                return new LivestreamProductDTO
                {
                    Id = livestreamProduct.Id,
                    LivestreamId = livestreamProduct.LivestreamId,
                    ProductId = livestreamProduct.ProductId,
                    VariantId = livestreamProduct.VariantId,
                    SKU = sku,
                    ProductName = product?.ProductName ?? "Unknown Product",
                    Price = livestreamProduct.Price,
                    Stock = livestreamProduct.Stock,
                    ProductStock = productStock,
                    IsPin = livestreamProduct.IsPin,
                    CreatedAt = livestreamProduct.CreatedAt,
                    LastModifiedAt = livestreamProduct.LastModifiedAt,
                    ProductImageUrl = product?.PrimaryImageUrl ?? product?.ImageUrl ?? string.Empty,
                    VariantName = variantName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pinning product by ID {Id}", request.Id);
                throw;
            }
        }
    }
}