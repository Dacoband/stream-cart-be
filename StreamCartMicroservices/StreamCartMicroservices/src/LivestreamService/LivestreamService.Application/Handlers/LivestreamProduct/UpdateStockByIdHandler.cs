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
    public class UpdateStockByIdHandler : IRequestHandler<UpdateStockByIdCommand, LivestreamProductDTO>
    {
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<UpdateStockByIdHandler> _logger;

        public UpdateStockByIdHandler(
            ILivestreamProductRepository livestreamProductRepository,
            ILivestreamRepository livestreamRepository,
            IProductServiceClient productServiceClient,
            ILogger<UpdateStockByIdHandler> logger)
        {
            _livestreamProductRepository = livestreamProductRepository;
            _livestreamRepository = livestreamRepository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        // ✅ Fix: Correct return type
        public async Task<LivestreamProductDTO> Handle(UpdateStockByIdCommand request, CancellationToken cancellationToken)
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
                    throw new UnauthorizedAccessException("You can only update stock for products in your own livestream");
                }

                // Update stock
                livestreamProduct.UpdateStock(request.Stock, request.SellerId.ToString());
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
                    // ✅ Fix: Correct property mapping based on ProductVariantDTO structure
                    ProductImageUrl = product?.ImageUrl ?? string.Empty,
                    VariantName = variant?.Name ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock by ID {Id}", request.Id);
                throw;
            }
        }
    }
}