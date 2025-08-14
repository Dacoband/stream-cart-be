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

                // Verify seller owns the livestream
                var livestream = await _livestreamRepository.GetByIdAsync(livestreamProduct.LivestreamId.ToString());
                if (livestream == null)
                {
                    throw new KeyNotFoundException("Livestream not found");
                }

                if (livestream.SellerId != request.SellerId)
                {
                    throw new UnauthorizedAccessException("You can only pin products in your own livestream");
                }

                // Handle pinning logic
                if (request.IsPin)
                {
                    // Check if there's already a pinned product
                    var currentPinnedProduct = await _livestreamProductRepository.GetCurrentPinnedProductAsync(livestreamProduct.LivestreamId);
                    if (currentPinnedProduct != null && currentPinnedProduct.Id != livestreamProduct.Id)
                    {
                        // Unpin the current pinned product
                        currentPinnedProduct.SetPin(false, request.SellerId.ToString());
                        await _livestreamProductRepository.ReplaceAsync(currentPinnedProduct.Id.ToString(), currentPinnedProduct);
                    }
                }

                // Update pin status
                livestreamProduct.SetPin(request.IsPin, request.SellerId.ToString());
                await _livestreamProductRepository.ReplaceAsync(livestreamProduct.Id.ToString(), livestreamProduct);

                // Get product details for response
                var product = await _productServiceClient.GetProductByIdAsync(livestreamProduct.ProductId);

                ProductVariantDTO? variant = null;
                if (!string.IsNullOrEmpty(livestreamProduct.VariantId))
                {
                    variant = await _productServiceClient.GetProductVariantAsync(
                        livestreamProduct.ProductId, livestreamProduct.VariantId);
                }

                return new LivestreamProductDTO
                {
                    Id = livestreamProduct.Id,
                    LivestreamId = livestreamProduct.LivestreamId,
                    ProductId = livestreamProduct.ProductId,
                    VariantId = livestreamProduct.VariantId,
                    SKU = variant?.SKU ?? product?.SKU ?? string.Empty,
                    ProductName = product?.ProductName ?? "Unknown Product",
                    Price = livestreamProduct.Price,
                    Stock = livestreamProduct.Stock,
                    ProductStock = product?.StockQuantity ?? 0,
                    IsPin = livestreamProduct.IsPin,
                    CreatedAt = livestreamProduct.CreatedAt,
                    LastModifiedAt = livestreamProduct.LastModifiedAt,
                    // ✅ Fix: Correct property mapping
                    ProductImageUrl = product?.PrimaryImageUrl ?? string.Empty,
                    VariantName = variant?.Name ?? string.Empty
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