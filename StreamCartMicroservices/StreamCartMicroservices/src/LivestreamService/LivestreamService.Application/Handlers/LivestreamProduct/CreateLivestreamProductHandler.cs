using LivestreamService.Application.Commands;
using LivestreamService.Application.Commands.LiveStreamService;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.LivestreamProduct
{
    public class CreateLivestreamProductHandler : IRequestHandler<CreateLivestreamProductCommand, LivestreamProductDTO>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<CreateLivestreamProductHandler> _logger;

        public CreateLivestreamProductHandler(
            ILivestreamRepository livestreamRepository,
            ILivestreamProductRepository livestreamProductRepository,
            IProductServiceClient productServiceClient,
            ILogger<CreateLivestreamProductHandler> logger)
        {
            _livestreamRepository = livestreamRepository;
            _livestreamProductRepository = livestreamProductRepository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        public async Task<LivestreamProductDTO> Handle(CreateLivestreamProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Kiểm tra livestream tồn tại
                var livestream = await _livestreamRepository.GetByIdAsync(request.LivestreamId.ToString());
                if (livestream == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy livestream với ID {request.LivestreamId}");
                }

                // Xác minh người bán chính là chủ sở hữu của livestream
                if (livestream.SellerId != request.SellerId)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền thêm sản phẩm vào livestream này");
                }

                // Kiểm tra sản phẩm thuộc về shop của livestream
                var isProductOwnedByShop = await _productServiceClient.IsProductOwnedByShopAsync(
                    request.ProductId,
                    livestream.ShopId);

                if (!isProductOwnedByShop)
                {
                    throw new UnauthorizedAccessException("Sản phẩm không thuộc về shop của livestream này");
                }

                // Kiểm tra sản phẩm và variant đã tồn tại trong livestream chưa
                bool productExists = await _livestreamProductRepository.ExistsByProductInLivestreamAsync(
                    request.LivestreamId,
                    request.ProductId,
                    request.VariantId);

                if (productExists)
                {
                    throw new InvalidOperationException("Sản phẩm đã tồn tại trong livestream này");
                }

                // Lấy thông tin sản phẩm từ Product Service để kiểm tra tồn kho và giá
                var product = await _productServiceClient.GetProductByIdAsync(request.ProductId);

                // Kiểm tra variant nếu có
                ProductVariantDTO variant = null;
                if (!string.IsNullOrEmpty(request.VariantId))
                {
                    variant = await _productServiceClient.GetProductVariantAsync(request.ProductId, request.VariantId);
                }
                if (request.FlashSaleId.HasValue)
                {
                    // Cần thêm method này vào IProductServiceClient
                    var flashSale = await _productServiceClient.GetFlashSaleByIdAsync(request.FlashSaleId.Value);
                    if (flashSale == null)
                    {
                        throw new KeyNotFoundException($"Không tìm thấy FlashSale với ID {request.FlashSaleId}");
                    }

                    // Kiểm tra FlashSale có áp dụng cho sản phẩm/variant này không
                    if (flashSale.ProductId.ToString() != request.ProductId)
                    {
                        throw new InvalidOperationException("FlashSale không áp dụng cho sản phẩm này");
                    }

                    if (!string.IsNullOrEmpty(request.VariantId) &&
                        flashSale.VariantId?.ToString() != request.VariantId)
                    {
                        throw new InvalidOperationException("FlashSale không áp dụng cho variant này");
                    }

                    // Kiểm tra FlashSale còn hiệu lực không
                    var now = DateTime.UtcNow;
                    if (flashSale.StartTime > now || flashSale.EndTime < now)
                    {
                        throw new InvalidOperationException("FlashSale không còn hiệu lực");
                    }
                }
                // Inside the Handle method
                var livestreamProduct = new LivestreamService.Domain.Entities.LivestreamProduct(
                    request.LivestreamId,
                    request.ProductId,
                    request.VariantId ?? string.Empty,
                    request.Price,
                    request.Stock,
                    request.FlashSaleId,
                    request.IsPin,
                    0,
                    request.SellerId.ToString()
                );

                // Lưu vào database
                await _livestreamProductRepository.InsertAsync(livestreamProduct);

                // Trả về DTO
                return new LivestreamProductDTO
                {
                    Id = livestreamProduct.Id,
                    LivestreamId = livestreamProduct.LivestreamId,
                    ProductId = livestreamProduct.ProductId,
                    VariantId = livestreamProduct.VariantId,
                    FlashSaleId = livestreamProduct.FlashSaleId,
                    IsPin = livestreamProduct.IsPin,
                    Price = livestreamProduct.Price,
                    Stock = livestreamProduct.Stock,
                    CreatedAt = livestreamProduct.CreatedAt,
                    LastModifiedAt = livestreamProduct.LastModifiedAt,
                    ProductName = product.Name,
                    ProductImageUrl = product.ImageUrl,
                    VariantName = variant?.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo sản phẩm cho livestream");
                throw;
            }
        }
    }
}