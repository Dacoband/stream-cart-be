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
    public class GetBestSellingProductsHandler : IRequestHandler<GetBestSellingProductsQuery, IEnumerable<LivestreamProductSummaryDTO>>
    {
        private readonly ILivestreamProductRepository _repository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetBestSellingProductsHandler> _logger;

        public GetBestSellingProductsHandler(
            ILivestreamProductRepository repository,
            IProductServiceClient productServiceClient,
            ILogger<GetBestSellingProductsHandler> logger)
        {
            _repository = repository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<IEnumerable<LivestreamProductSummaryDTO>> Handle(GetBestSellingProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var livestreamProducts = await _repository.GetByLivestreamIdAsync(request.LivestreamId);
                var result = new List<LivestreamProductSummaryDTO>();

                foreach (var lp in livestreamProducts)
                {
                    var product = await _productServiceClient.GetProductByIdAsync(lp.ProductId);
                    if (product != null)
                    {
                        result.Add(new LivestreamProductSummaryDTO
                        {
                            Id = lp.Id,
                            ProductId = lp.ProductId,
                            ProductName = product.ProductName,
                            ProductImageUrl = product.ImageUrl,
                           // OriginalPrice = product.OriginalPrice,
                            Price = lp.Price,
                            Stock = lp.Stock,
                            IsPin = lp.IsPin,
                           // HasFlashSale = lp.FlashSaleId.HasValue,
                            SoldQuantity = product.QuantitySold ?? 0,
                            //DisplayOrder = lp.DisplayOrder
                        });
                    }
                }

                // Sort by sold quantity (best selling first)
                return result.OrderByDescending(x => x.SoldQuantity).Take(request.Limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting best selling products for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}