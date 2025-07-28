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

namespace LivestreamService.Application.Handlers.LivestreamProductHandlers
{
    public class GetPinnedProductsHandler : IRequestHandler<GetPinnedProductsQuery, IEnumerable<LivestreamProductDTO>>
    {
        private readonly ILivestreamProductRepository _repository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetPinnedProductsHandler> _logger;

        public GetPinnedProductsHandler(
            ILivestreamProductRepository repository,
            IProductServiceClient productServiceClient,
            ILogger<GetPinnedProductsHandler> logger)
        {
            _repository = repository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<IEnumerable<LivestreamProductDTO>> Handle(GetPinnedProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var pinnedProducts = await _repository.GetPinnedProductsAsync(request.LivestreamId, request.Limit);
                var result = new List<LivestreamProductDTO>();

                foreach (var lp in pinnedProducts)
                {
                    var product = await _productServiceClient.GetProductByIdAsync(lp.ProductId);
                    if (product != null)
                    {
                        ProductVariantDTO? variant = null;
                        if (!string.IsNullOrEmpty(lp.VariantId))
                        {
                            variant = await _productServiceClient.GetProductVariantAsync(lp.ProductId, lp.VariantId);
                        }

                        result.Add(new LivestreamProductDTO
                        {
                            Id = lp.Id,
                            LivestreamId = lp.LivestreamId,
                            ProductId = lp.ProductId,
                            VariantId = lp.VariantId,
                            FlashSaleId = lp.FlashSaleId,
                            IsPin = lp.IsPin,
                            Price = lp.Price,
                            Stock = lp.Stock,
                            CreatedAt = lp.CreatedAt,
                            LastModifiedAt = lp.LastModifiedAt,
                            ProductName = product.Name,
                            ProductImageUrl = product.ImageUrl,
                            VariantName = variant?.Name
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pinned products for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}