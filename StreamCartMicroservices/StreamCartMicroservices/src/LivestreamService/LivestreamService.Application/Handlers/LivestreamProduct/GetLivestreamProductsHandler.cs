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

namespace LivestreamService.Application.Handlers.LivestreamProduct
{
    public class GetLivestreamProductsHandler : IRequestHandler<GetLivestreamProductsQuery, IEnumerable<LivestreamProductDTO>>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetLivestreamProductsHandler> _logger;

        public GetLivestreamProductsHandler(
            ILivestreamRepository livestreamRepository,
            ILivestreamProductRepository livestreamProductRepository,
            IProductServiceClient productServiceClient,
            ILogger<GetLivestreamProductsHandler> logger)
        {
            _livestreamRepository = livestreamRepository;
            _livestreamProductRepository = livestreamProductRepository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<IEnumerable<LivestreamProductDTO>> Handle(GetLivestreamProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Kiểm tra livestream tồn tại
                var livestream = await _livestreamRepository.GetByIdAsync(request.LivestreamId.ToString());
                if (livestream == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy livestream với ID {request.LivestreamId}");
                }

                // Lấy danh sách sản phẩm
                var products = await _livestreamProductRepository.GetByLivestreamIdAsync(request.LivestreamId);

                // Chuyển đổi sang DTO
                var result = new List<LivestreamProductDTO>();

                foreach (var product in products)
                {
                    try
                    {
                        // Lấy thông tin sản phẩm từ Product Service
                        var productInfo = await _productServiceClient.GetProductByIdAsync(product.ProductId);

                        // Lấy thông tin variant nếu có
                        ProductVariantDTO variantInfo = null;
                        if (!string.IsNullOrEmpty(product.VariantId))
                        {
                            variantInfo = await _productServiceClient.GetProductVariantAsync(product.ProductId, product.VariantId);
                        }

                        result.Add(new LivestreamProductDTO
                        {
                            Id = product.Id,
                            LivestreamId = product.LivestreamId,
                            ProductId = product.ProductId,
                            VariantId = product.VariantId,
                            FlashSaleId = product.FlashSaleId,
                            IsPin = product.IsPin,
                            Price = product.Price,
                            Stock = product.Stock,
                            CreatedAt = product.CreatedAt,
                            LastModifiedAt = product.LastModifiedAt,
                            ProductName = productInfo?.Name ?? "Không rõ",
                            ProductImageUrl = productInfo?.ImageUrl ?? "",
                            VariantName = variantInfo?.Name ?? ""
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Không thể lấy thông tin chi tiết cho sản phẩm {ProductId}", product.ProductId);

                        // Vẫn trả về thông tin cơ bản nếu không thể lấy thông tin chi tiết
                        result.Add(new LivestreamProductDTO
                        {
                            Id = product.Id,
                            LivestreamId = product.LivestreamId,
                            ProductId = product.ProductId,
                            VariantId = product.VariantId,
                            FlashSaleId = product.FlashSaleId,
                            IsPin = product.IsPin,
                            Price = product.Price,
                            Stock = product.Stock,
                            CreatedAt = product.CreatedAt,
                            LastModifiedAt = product.LastModifiedAt,
                            ProductName = "Không thể lấy thông tin",
                            ProductImageUrl = "",
                            VariantName = ""
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sản phẩm của livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}