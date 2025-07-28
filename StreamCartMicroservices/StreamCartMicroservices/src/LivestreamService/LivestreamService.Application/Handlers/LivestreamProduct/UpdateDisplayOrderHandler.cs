using LivestreamService.Application.Commands.LiveStreamService;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.LivestreamProductHandlers
{
    public class UpdateDisplayOrderHandler : IRequestHandler<UpdateDisplayOrderCommand, LivestreamProductDTO>
    {
        private readonly ILivestreamProductRepository _repository;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<UpdateDisplayOrderHandler> _logger;

        public UpdateDisplayOrderHandler(
            ILivestreamProductRepository repository,
            ILivestreamRepository livestreamRepository,
            IProductServiceClient productServiceClient,
            ILogger<UpdateDisplayOrderHandler> logger)
        {
            _repository = repository;
            _livestreamRepository = livestreamRepository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<LivestreamProductDTO> Handle(UpdateDisplayOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var livestreamProduct = await _repository.GetLivestreamProductAsync(request.Id);
                if (livestreamProduct == null)
                    throw new KeyNotFoundException($"Livestream product with ID {request.Id} not found");

                // Verify seller owns the livestream
                var livestream = await _livestreamRepository.GetByIdAsync(livestreamProduct.LivestreamId.ToString());
                if (livestream == null)
                    throw new KeyNotFoundException("Livestream not found");

                if (livestream.SellerId != request.SellerId)
                    throw new UnauthorizedAccessException("You can only update products in your own livestream");

                
                await _repository.ReplaceAsync(livestreamProduct.Id.ToString(), livestreamProduct);

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
                    ProductName = product?.Name,
                    ProductImageUrl = product?.ImageUrl,
                    VariantName = variant?.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating display order for livestream product {Id}", request.Id);
                throw;
            }
        }
    }
}