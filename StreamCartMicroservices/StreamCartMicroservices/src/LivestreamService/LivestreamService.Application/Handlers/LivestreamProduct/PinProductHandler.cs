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
    public class PinProductHandler : IRequestHandler<PinProductCommand, LivestreamProductDTO>
    {
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<PinProductHandler> _logger;

        public PinProductHandler(
            ILivestreamProductRepository livestreamProductRepository,
            ILivestreamRepository livestreamRepository,
            IProductServiceClient productServiceClient,
            ILogger<PinProductHandler> logger)
        {
            _livestreamProductRepository = livestreamProductRepository;
            _livestreamRepository = livestreamRepository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<LivestreamProductDTO> Handle(PinProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Find product by composite key or ID (depending on your command structure)
                LivestreamService.Domain.Entities.LivestreamProduct? livestreamProduct;

                if (request.LivestreamId != Guid.Empty && !string.IsNullOrEmpty(request.ProductId))
                {
                    // Use composite key approach
                    livestreamProduct = await _livestreamProductRepository.GetByCompositeKeyAsync(
                        request.LivestreamId, request.ProductId, request.VariantId ?? string.Empty);
                }
                else
                {
                    // Fallback to ID approach (if still needed)
                    livestreamProduct = await _livestreamProductRepository.GetLivestreamProductAsync(request.Id);
                }

                if (livestreamProduct == null)
                {
                    throw new KeyNotFoundException($"Livestream product not found");
                }

                // Verify seller owns the livestream
                var livestream = await _livestreamRepository.GetByIdAsync(livestreamProduct.LivestreamId.ToString());
                if (livestream == null)
                {
                    throw new KeyNotFoundException("Livestream not found");
                }

                if (livestream.SellerId != request.SellerId)
                {
                    throw new UnauthorizedAccessException("You can only pin/unpin products in your own livestream");
                }

                // ✅ IMPLEMENT SMART PINNING LOGIC - Only one product can be pinned at a time
                if (request.IsPin)
                {
                    // Check if there's already a pinned product
                    var currentPinnedProduct = await _livestreamProductRepository.GetCurrentPinnedProductAsync(livestreamProduct.LivestreamId);

                    if (currentPinnedProduct != null && currentPinnedProduct.Id != livestreamProduct.Id)
                    {
                        // Unpin the currently pinned product
                        currentPinnedProduct.SetPin(false, request.SellerId.ToString());
                        await _livestreamProductRepository.ReplaceAsync(currentPinnedProduct.Id.ToString(), currentPinnedProduct);

                        _logger.LogInformation("Unpinned previous product {ProductId} in livestream {LivestreamId} before pinning new product {NewProductId}",
                            currentPinnedProduct.ProductId, livestreamProduct.LivestreamId, livestreamProduct.ProductId);
                    }

                    // Pin the new product
                    livestreamProduct.SetPin(true, request.SellerId.ToString());
                    _logger.LogInformation("Pinned product {ProductId} in livestream {LivestreamId}",
                        livestreamProduct.ProductId, livestreamProduct.LivestreamId);
                }
                else
                {
                    // Simply unpin this product
                    livestreamProduct.SetPin(false, request.SellerId.ToString());
                    _logger.LogInformation("Unpinned product {ProductId} in livestream {LivestreamId}",
                        livestreamProduct.ProductId, livestreamProduct.LivestreamId);
                }

                // Save changes
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
                    IsPin = livestreamProduct.IsPin,
                    Price = livestreamProduct.Price,
                    Stock = livestreamProduct.Stock,
                    CreatedAt = livestreamProduct.CreatedAt,
                    LastModifiedAt = livestreamProduct.LastModifiedAt,
                    ProductName = product?.ProductName,
                    ProductImageUrl = product?.ImageUrl,
                    VariantName = variant?.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pinning/unpinning livestream product");
                throw;
            }
        }
    }
}