using LivestreamService.Application.Commands;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers
{
    public class CreateLivestreamCommandHandler : IRequestHandler<CreateLivestreamCommand, LivestreamDTO>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly ILivekitService _livekitService;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<CreateLivestreamCommandHandler> _logger;

        public CreateLivestreamCommandHandler(
            ILivestreamRepository livestreamRepository,
            ILivestreamProductRepository livestreamProductRepository,
            ILivekitService livekitService,
            IShopServiceClient shopServiceClient,
            IAccountServiceClient accountServiceClient,
            IProductServiceClient productServiceClient,
            ILogger<CreateLivestreamCommandHandler> logger)
        {
            _livestreamRepository = livestreamRepository ?? throw new ArgumentNullException(nameof(livestreamRepository));
            _livestreamProductRepository = livestreamProductRepository ?? throw new ArgumentNullException(nameof(livestreamProductRepository));
            _livekitService = livekitService ?? throw new ArgumentNullException(nameof(livekitService));
            _shopServiceClient = shopServiceClient ?? throw new ArgumentNullException(nameof(shopServiceClient));
            _accountServiceClient = accountServiceClient ?? throw new ArgumentNullException(nameof(accountServiceClient));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LivestreamDTO> Handle(CreateLivestreamCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate shop
                var shop = await _shopServiceClient.GetShopByIdAsync(request.ShopId);
                if (shop == null)
                {
                    throw new ApplicationException($"Shop with ID {request.ShopId} not found");
                }

                // Verify seller account
                var seller = await _accountServiceClient.GetAccountByIdAsync(request.SellerId);
                if (seller == null)
                {
                    throw new ApplicationException($"Seller account with ID {request.SellerId} not found");
                }

                // ✅ VALIDATE SẢN PHẨM TRƯỚC KHI TẠO LIVESTREAM
                if (request.Products != null && request.Products.Any())
                {
                    await ValidateProductsAsync(request.Products, request.ShopId);
                }

                // Create LiveKit room
                string roomId = $"shop-{request.ShopId}-{Guid.NewGuid()}";
                string livekitRoomId = await _livekitService.CreateRoomAsync(roomId);
                string joinToken = await _livekitService.GenerateJoinTokenAsync(
                    livekitRoomId,
                    request.SellerId.ToString(),
                    true // Can publish
                );
                // Generate unique stream key
                string streamKey = Guid.NewGuid().ToString("N");

                // Create livestream entity
                var livestream = new Livestream(
                    request.Title,
                    request.Description,
                    request.SellerId,
                    request.ShopId,
                    request.ScheduledStartTime,
                    livekitRoomId,
                    streamKey,
                    request.ThumbnailUrl,
                    request.Tags,
                    joinToken,
                    request.SellerId.ToString()
                );

                // Set a default value for PlaybackUrl which is required by the database
                livestream.SetPlaybackUrl($"https://stream.placeholder.com/{livekitRoomId}");

                // Save livestream
                await _livestreamRepository.InsertAsync(livestream);

                // ✅ TẠO SẢN PHẨM CHO LIVESTREAM
                var createdProducts = new List<LivestreamProductDTO>();
                if (request.Products != null && request.Products.Any())
                {
                    createdProducts = await CreateLivestreamProductsAsync(livestream.Id, request.Products, request.SellerId);
                }

                // Generate join token for the seller (with publisher permissions)
                

                // Return DTO
                return new LivestreamDTO
                {
                    Id = livestream.Id,
                    Title = livestream.Title,
                    Description = livestream.Description,
                    SellerId = livestream.SellerId,
                    SellerName = seller.Fullname ?? seller.Username,
                    ShopId = livestream.ShopId,
                    ShopName = shop.ShopName,
                    ScheduledStartTime = livestream.ScheduledStartTime,
                    ActualStartTime = livestream.ActualStartTime,
                    ActualEndTime = livestream.ActualEndTime,
                    Status = livestream.Status,
                    StreamKey = livestream.StreamKey,
                    LivekitRoomId = livekitRoomId,
                    JoinToken = joinToken,
                    ThumbnailUrl = livestream.ThumbnailUrl,
                    Tags = livestream.Tags,
                    MaxViewer = livestream.MaxViewer,
                    ApprovalStatusContent = livestream.ApprovalStatusContent,
                    ApprovedByUserId = livestream.ApprovedByUserId,
                    ApprovalDateContent = livestream.ApprovalDateContent,
                    IsPromoted = livestream.IsPromoted,
                    Products = createdProducts
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating livestream: {Message}", ex.Message);
                throw;
            }
        }

        private async Task ValidateProductsAsync(List<CreateLivestreamProductItemDTO> products, Guid shopId)
        {
            foreach (var productItem in products)
            {
                // Kiểm tra sản phẩm tồn tại và thuộc shop
                var isOwned = await _productServiceClient.IsProductOwnedByShopAsync(productItem.ProductId, shopId);
                if (!isOwned)
                {
                    throw new ArgumentException($"Product {productItem.ProductId} does not belong to shop {shopId}");
                }

                // Kiểm tra variant nếu có
                if (!string.IsNullOrEmpty(productItem.VariantId))
                {
                    var variant = await _productServiceClient.GetProductVariantAsync(productItem.ProductId, productItem.VariantId);
                    if (variant == null)
                    {
                        throw new ArgumentException($"Variant {productItem.VariantId} not found for product {productItem.ProductId}");
                    }
                }
            }
        }

        private async Task<List<LivestreamProductDTO>> CreateLivestreamProductsAsync(
            Guid livestreamId,
            List<CreateLivestreamProductItemDTO> products,
            Guid sellerId)
        {
            var result = new List<LivestreamProductDTO>();

            foreach (var productItem in products)
            {
                try
                {
                    // Lấy thông tin sản phẩm
                    var product = await _productServiceClient.GetProductByIdAsync(productItem.ProductId);
                    ProductVariantDTO? variant = null;

                    if (!string.IsNullOrEmpty(productItem.VariantId))
                    {
                        variant = await _productServiceClient.GetProductVariantAsync(productItem.ProductId, productItem.VariantId);
                    }

                    // Xác định giá và stock
                    decimal finalPrice = productItem.Price ?? (variant?.Price ?? product?.BasePrice ?? 0);
                    int finalStock = productItem.Stock ?? (variant?.Stock ?? product?.StockQuantity ?? 0);

                    // ✅ FIX: Sử dụng đường dẫn đầy đủ để tránh namespace conflict
                    var livestreamProduct = new LivestreamService.Domain.Entities.LivestreamProduct(
                        livestreamId,
                        productItem.ProductId,
                        productItem.VariantId ?? string.Empty,
                        finalPrice,
                        finalStock,
                        productItem.IsPin,
                        sellerId.ToString()
                    );

                    await _livestreamProductRepository.InsertAsync(livestreamProduct);

                    // Tạo DTO để trả về
                    result.Add(new LivestreamProductDTO
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
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating livestream product for product {ProductId}", productItem.ProductId);
                    // Tiếp tục với sản phẩm khác thay vì throw exception
                }
            }

            return result;
        }
    }
}