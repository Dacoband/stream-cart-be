using LivestreamService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System.ComponentModel.DataAnnotations;
using DomainCartItem = LivestreamService.Domain.Entities.LivestreamCartItem;

namespace LivestreamService.Api.Controllers
{
    [ApiController]
    [Route("api/livestream-carts")]
    public class LivestreamCartController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ILivestreamCartRepository _livestreamCartRepository;
        private readonly ILivestreamCartItemRepository _livestreamCartItemRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<LivestreamCartController> _logger;

        public LivestreamCartController(
            ICurrentUserService currentUserService,
            ILivestreamCartRepository livestreamCartRepository,
            ILivestreamCartItemRepository livestreamCartItemRepository,
            IProductServiceClient productServiceClient,
            ILogger<LivestreamCartController> logger)
        {
            _currentUserService = currentUserService;
            _livestreamCartRepository = livestreamCartRepository;
            _livestreamCartItemRepository = livestreamCartItemRepository;
            _productServiceClient = productServiceClient;
            _logger = logger;
        }

        [HttpGet("PreviewOrder")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(typeof(ApiResponse<PreviewOrderResponseDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> PreviewOrderLive([FromQuery] PreviewOrderRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu nhập vào không hợp lệ"));

            try
            {
                if (request.CartItemId == null || !request.CartItemId.Any())
                {
                    return Ok(ApiResponse<PreviewOrderResponseDTO>.SuccessResult(new PreviewOrderResponseDTO
                    {
                        LivestreamId = Guid.Empty,
                        TotalAmount = 0,
                        Discount = 0,
                        TotalItem = 0,
                        SubTotal = 0,
                        ListCartItem = new List<ProductInShopCart>()
                    }, "Không tìm thấy sản phẩm nào trong giỏ hàng"));
                }

                var currentUserId = _currentUserService.GetUserId();

                // Lấy từng cart item theo ID
                var items = new List<DomainCartItem>();
                foreach (var id in request.CartItemId.Distinct())
                {
                    var it = await _livestreamCartItemRepository.GetByIdAsync(id.ToString());
                    if (it != null)
                        items.Add(it);
                }

                if (!items.Any())
                {
                    return Ok(ApiResponse<PreviewOrderResponseDTO>.SuccessResult(new PreviewOrderResponseDTO
                    {
                        LivestreamId = Guid.Empty,
                        TotalAmount = 0,
                        Discount = 0,
                        TotalItem = 0,
                        SubTotal = 0,
                        ListCartItem = new List<ProductInShopCart>()
                    }, "Không tìm thấy giỏ hàng"));
                }

                var livestreamId = items.First().LivestreamId;
                var cartId = items.First().LivestreamCartId;

                if (items.Any(i => i.LivestreamId != livestreamId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Các sản phẩm không cùng một livestream"));
                }
                if (items.Any(i => i.LivestreamCartId != cartId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Các sản phẩm không thuộc cùng một giỏ hàng"));
                }

                var cart = await _livestreamCartRepository.GetWithItemsAsync(cartId);
                if (cart == null || cart.ViewerId != currentUserId)
                {
                    return Forbid();
                }

                var grouped = new List<ProductInShopCart>();
                foreach (var shopGroup in items
                    .OrderByDescending(x => x.CreatedAt)
                    .GroupBy(ci => new { ci.ShopId, ci.ShopName }))
                {
                    var productList = new List<ProductCart>();
                    foreach (var ci in shopGroup)
                    {
                        var original = ci.OriginalPrice;
                        var current = ci.LivestreamPrice;
                        var discountPercent = original > 0 ? (original - current) / original * 100 : 0;

                        // Lấy kích thước/khối lượng từ Product Service (method mới)
                        decimal? weight = null, length = null, width = null, height = null;
                        if (!string.IsNullOrWhiteSpace(ci.VariantId))
                        {
                            var variant = await _productServiceClient.GetProductVariantWithDimensionsAsync(ci.ProductId, ci.VariantId);
                            if (variant != null)
                            {
                                weight = variant.Weight;
                                length = variant.Length;
                                width = variant.Width;
                                height = variant.Height;
                            }
                        }
                       else
                        {
                            var product = await _productServiceClient.GetProductByIdAsync(ci.ProductId);
                            if (product != null)
                            {
                                weight = product.Weight;
                                length = product.Length;
                                width = product.Width;
                                height = product.Height;
                            }
                        }


                        productList.Add(new ProductCart
                        {
                            CartItemId = ci.Id,
                            ProductId = Guid.TryParse(ci.ProductId, out var parsedPid) ? parsedPid : Guid.Empty,
                            VariantID = Guid.TryParse(ci.VariantId, out var parsedVid) ? parsedVid : (Guid?)null,
                            ProductName = ci.ProductName,
                            PriceData = new PriceData
                            {
                                CurrentPrice = current,
                                OriginalPrice = original,
                                Discount = discountPercent
                            },
                            Quantity = ci.Quantity,
                            StockQuantity = ci.Stock,
                            Attributes = ci.Attributes,
                            PrimaryImage = ci.PrimaryImage,
                            ProductStatus = ci.ProductStatus,
                            Weight = weight,
                            Length = length,
                            Width = width,
                            Height = height
                        });
                    }

                    grouped.Add(new ProductInShopCart
                    {
                        ShopId = shopGroup.Key.ShopId,
                        ShopName = shopGroup.Key.ShopName,
                        Products = productList,
                        NumberOfProduct = productList.Sum(p => p.Quantity),
                        TotalPriceInShop = productList.Sum(p => p.PriceData.CurrentPrice * p.Quantity)
                    });
                }

                // Tổng hợp (match CartService)
                var totalItem = items.Sum(ci => ci.Quantity);
                var totalAmount = items.Sum(ci => ci.LivestreamPrice * ci.Quantity);
                var discount = items.Sum(x => (x.OriginalPrice - x.LivestreamPrice) * x.Quantity);
                var subTotal = items.Sum(x => x.OriginalPrice * x.Quantity);

                var response = new PreviewOrderResponseDTO
                {
                    LivestreamId = livestreamId,
                    TotalAmount = totalAmount,
                    Discount = discount,
                    TotalItem = totalItem,
                    SubTotal = subTotal,
                    ListCartItem = grouped
                };

                return Ok(ApiResponse<PreviewOrderResponseDTO>.SuccessResult(response, "Tạo PreviewOrder"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi preview order live");
                return BadRequest(ApiResponse<object>.ErrorResult("Lỗi khi preview order live"));
            }
        }
    }

    // Request: y hệt Cart PreviewOrder (chỉ có CartItemId)
    public class PreviewOrderRequestDTO
    {
        [Required]
        public List<Guid> CartItemId { get; set; } = new();
    }

    // Response: giữ nguyên shape như CartService + LivestreamId
    public class PreviewOrderResponseDTO
    {
        public Guid LivestreamId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public int TotalItem { get; set; }
        public decimal SubTotal { get; set; }
        public List<ProductInShopCart> ListCartItem { get; set; } = new();
    }

    public class ProductInShopCart
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public List<ProductCart> Products { get; set; } = new();
        public int NumberOfProduct { get; set; }
        public decimal TotalPriceInShop { get; set; }
    }

    public class ProductCart
    {
        public Guid CartItemId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public PriceData PriceData { get; set; } = new();
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
        public string? PrimaryImage { get; set; }
        public bool ProductStatus { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
    }

    public class PriceData
    {
        public decimal CurrentPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal Discount { get; set; } // %
    }
}