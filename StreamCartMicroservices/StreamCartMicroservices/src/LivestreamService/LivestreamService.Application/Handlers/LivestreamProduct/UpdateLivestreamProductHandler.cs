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
    public class UpdateLivestreamProductHandler : IRequestHandler<UpdateLivestreamProductCommand, LivestreamProductDTO>
    {
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<UpdateLivestreamProductHandler> _logger;

        public UpdateLivestreamProductHandler(
            ILivestreamProductRepository livestreamProductRepository,
            ILivestreamRepository livestreamRepository,
            IProductServiceClient productServiceClient,
            ILogger<UpdateLivestreamProductHandler> logger)
        {
            _livestreamProductRepository = livestreamProductRepository;
            _livestreamRepository = livestreamRepository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<LivestreamProductDTO> Handle(UpdateLivestreamProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // ✅ SỬ DỤNG COMPOSITE KEY THAY VÌ ID
                var livestreamProduct = await _livestreamProductRepository.GetByCompositeKeyAsync(
                    request.LivestreamId, request.ProductId, request.VariantId);

                if (livestreamProduct == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy sản phẩm trong livestream {request.LivestreamId}, ProductId: {request.ProductId}, VariantId: {request.VariantId}");
                }

                // Verify seller owns the livestream
                var livestream = await _livestreamRepository.GetByIdAsync(livestreamProduct.LivestreamId.ToString());
                if (livestream == null)
                {
                    throw new KeyNotFoundException("Livestream not found");
                }

               

                // Update properties if provided
                if (request.Price.HasValue)
                {
                    livestreamProduct.UpdatePrice(request.Price.Value, request.SellerId.ToString());
                }

                if (request.Stock.HasValue)
                {
                    livestreamProduct.UpdateStock(request.Stock.Value, request.SellerId.ToString());
                }

                // ✅ HANDLE PINNING LOGIC WITH SMART PIN MANAGEMENT
                if (request.IsPin.HasValue)
                {
                    if (request.IsPin.Value)
                    {
                        // Check if there's already a pinned product
                        var currentPinnedProduct = await _livestreamProductRepository.GetCurrentPinnedProductAsync(livestreamProduct.LivestreamId);

                        if (currentPinnedProduct != null && currentPinnedProduct.Id != livestreamProduct.Id)
                        {
                            // Unpin the currently pinned product
                            currentPinnedProduct.SetPin(false, request.SellerId.ToString());
                            await _livestreamProductRepository.ReplaceAsync(currentPinnedProduct.Id.ToString(), currentPinnedProduct);

                            _logger.LogInformation("Unpinned previous product {ProductId} when pinning new product {NewProductId} in livestream {LivestreamId}",
                                currentPinnedProduct.ProductId, livestreamProduct.ProductId, livestreamProduct.LivestreamId);
                        }
                    }

                    livestreamProduct.SetPin(request.IsPin.Value, request.SellerId.ToString());
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
                _logger.LogError(ex, "Error updating livestream product for LivestreamId: {LivestreamId}, ProductId: {ProductId}, VariantId: {VariantId}",
                    request.LivestreamId, request.ProductId, request.VariantId);
                throw;
            }
        }
    }
}