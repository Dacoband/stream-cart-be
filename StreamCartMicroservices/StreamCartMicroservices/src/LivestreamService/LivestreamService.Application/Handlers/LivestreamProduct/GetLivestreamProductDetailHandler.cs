using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.LivestreamProductHandlers
{
    public class GetLivestreamProductDetailHandler : IRequestHandler<GetLivestreamProductDetailQuery, LivestreamProductDetailDTO>
    {
        private readonly ILivestreamProductRepository _repository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<GetLivestreamProductDetailHandler> _logger;

        public GetLivestreamProductDetailHandler(
            ILivestreamProductRepository repository,
            IProductServiceClient productServiceClient,
            IShopServiceClient shopServiceClient,
            ILogger<GetLivestreamProductDetailHandler> logger)
        {
            _repository = repository;
            _productServiceClient = productServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
        }

        public async Task<LivestreamProductDetailDTO> Handle(GetLivestreamProductDetailQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var livestreamProduct = await _repository.GetLivestreamProductAsync(request.Id);
                if (livestreamProduct == null)
                    throw new KeyNotFoundException($"Livestream product with ID {request.Id} not found");

                // Get product details
                var product = await _productServiceClient.GetProductByIdAsync(livestreamProduct.ProductId);
                if (product == null)
                    throw new KeyNotFoundException($"Product {livestreamProduct.ProductId} not found");

                // Get variant details if exists
                ProductVariantDTO? variant = null;
                if (!string.IsNullOrEmpty(livestreamProduct.VariantId))
                {
                    variant = await _productServiceClient.GetProductVariantAsync(
                        livestreamProduct.ProductId, livestreamProduct.VariantId);
                }

                // Get shop details
                var shop = await _shopServiceClient.GetShopByIdAsync(product.ShopId);

                

                var result = new LivestreamProductDetailDTO
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

                    // Product info
                    ProductName = product.ProductName,
                    ProductDescription = product.Description,
                    ProductImageUrl = product.ImageUrl,
                    BasePrice = product.BasePrice,
                    ProductSoldQuantity = product.QuantitySold ?? 0,
                    ProductIsActive = product.IsActive,

                    // Variant info
                    VariantName = variant?.Name,

                    // Shop info
                    ShopId = product.ShopId,
                    ShopName = shop?.ShopName
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream product detail for ID {Id}", request.Id);
                throw;
            }
        }
    }
}