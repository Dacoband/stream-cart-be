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
    public class UpdateStockHandler : IRequestHandler<UpdateStockCommand, LivestreamProductDTO>
    {
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<UpdateStockHandler> _logger;

        public UpdateStockHandler(
            ILivestreamProductRepository livestreamProductRepository,
            ILivestreamRepository livestreamRepository,
            IProductServiceClient productServiceClient,
            ILogger<UpdateStockHandler> logger)
        {
            _livestreamProductRepository = livestreamProductRepository;
            _livestreamRepository = livestreamRepository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<LivestreamProductDTO> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // ✅ SỬ DỤNG COMPOSITE KEY
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

                // Update stock
                livestreamProduct.UpdateStock(request.Stock, request.SellerId.ToString());
                if (request.Price.HasValue && request.Price.Value > 0)
                {
                    livestreamProduct.UpdatePrice(request.Price.Value, request.SellerId.ToString());
                }
                else
                {
                    _logger.LogInformation("Chỉ cập nhật stock cho livestream product {ProductId}: Stock={Stock}",
                        livestreamProduct.Id, request.Stock);
                }

                livestreamProduct.SetModifier(request.SellerId.ToString());

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
                    ProductImageUrl = product?.PrimaryImageUrl ?? product?.ImageUrl ?? string.Empty,
                    VariantName = variant?.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for livestream product for LivestreamId: {LivestreamId}, ProductId: {ProductId}, VariantId: {VariantId}",
                    request.LivestreamId, request.ProductId, request.VariantId);
                throw;
            }
        }
    }
}