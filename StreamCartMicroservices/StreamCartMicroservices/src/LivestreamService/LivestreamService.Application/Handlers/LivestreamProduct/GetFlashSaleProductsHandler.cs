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
    public class GetFlashSaleProductsHandler : IRequestHandler<GetFlashSaleProductsQuery, IEnumerable<LivestreamProductDTO>>
    {
        private readonly ILivestreamProductRepository _repository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<GetFlashSaleProductsHandler> _logger;

        public GetFlashSaleProductsHandler(
            ILivestreamProductRepository repository,
            IProductServiceClient productServiceClient,
            IShopServiceClient shopServiceClient,
            ILogger<GetFlashSaleProductsHandler> logger)
        {
            _repository = repository;
            _productServiceClient = productServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
        }

        public async Task<IEnumerable<LivestreamProductDTO>> Handle(GetFlashSaleProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var flashSaleProducts = await _repository.GetFlashSaleProductsAsync(request.LivestreamId);
                var result = new List<LivestreamProductDTO>();

                foreach (var lp in flashSaleProducts)
                {
                    var product = await _productServiceClient.GetProductByIdAsync(lp.ProductId);
                    if (product != null)
                    {
                        var shop = await _shopServiceClient.GetShopByIdAsync(product.ShopId);

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
                            //FlashSaleId = lp.FlashSaleId,
                            IsPin = lp.IsPin,
                            Price = lp.Price,
                            Stock = lp.Stock,
                            CreatedAt = lp.CreatedAt,
                            LastModifiedAt = lp.LastModifiedAt,
                            ProductName = product.ProductName,
                            ProductImageUrl = product.ImageUrl,
                            VariantName = variant?.Name
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flash sale products for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}