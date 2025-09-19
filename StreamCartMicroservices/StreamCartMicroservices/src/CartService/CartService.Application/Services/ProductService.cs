//using CartService.Application.DTOs;
//using CartService.Application.Interfaces;
//using Microsoft.Extensions.Logging;
//using Shared.Common.Extensions;
//using Shared.Common.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http.Headers;
//using System.Net.Http.Json;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace CartService.Application.Services
//{
//    public class ProductService : IProductService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly ILogger<ProductService> _logger;
//        public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
//        {
//            // Tạo handler mới cho mỗi instance (chỉ dùng cho dev)
//            var handler = new HttpClientHandler
//            {
//                ServerCertificateCustomValidationCallback =
//                    (sender, cert, chain, sslPolicyErrors) => true
//            };

//            _httpClient = new HttpClient(handler);
//            _logger = logger;
//            _httpClient.DefaultRequestHeaders.Accept.Add(
//    new MediaTypeWithQualityHeaderValue("application/json"));
//        }

//        public async Task<ProductSnapshotDTO>? GetProductInfoAsync(string productId, string? variantId)
//        {
//            try { 
//            var url = $"https://brightpa.me/api/products/{productId}/detail";
//            var response = await _httpClient.GetAsync(url);

//            if (!response.IsSuccessStatusCode)
//            {
//                _logger.LogError($"API returned {response.StatusCode}");
//                return null;
//            }

//            var content = await response.Content.ReadAsStringAsync();
//            _logger.LogInformation($"Raw JSON response: {content}"); // Log raw JSON

//            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
//            var productResponse = JsonSerializer.Deserialize<ApiResponse<ProductDetailDto>>(content, options);
//                var product = productResponse.Data;
//            if (product == null)
//            {
//                _logger.LogError("Deserialization returned null");
//                return null;
//            }

//            ProductSnapshotDTO productSnapshot = new ProductSnapshotDTO()
//                {
//                    ProductId = product.ProductId,
//                    ProductName = product.ProductName,
//                    ShopName = product.ShopName,
//                    ShopId = product.ShopId,
//                    PriceOriginal = product.BasePrice,
//                    PriceCurrent = product.FinalPrice,
//                    Stock = product.StockQuantity,
//                    PrimaryImage = product.PrimaryImage?.FirstOrDefault() ?? "",
//                    Length = product.Length,
//                    Width = product.Width,
//                    Height = product.Height,
//                    Weight = product.Weight ?? 0,
//                };
//                if(product.Variants.Count > 0 && variantId.IsNullOrEmpty() && variantId.IsNullOrWhiteSpace())
//                {
//                    return null;
//                }

//                if (!variantId.IsNullOrEmpty() && !variantId.IsNullOrWhiteSpace())
//                {
//                    var variant = product.Variants.Where(x => x.VariantId.ToString() == variantId).FirstOrDefault();
//                    productSnapshot.PriceCurrent =
//                        (variant.FlashSalePrice.HasValue && variant.FlashSalePrice.Value > 0)
//                        ? variant.FlashSalePrice.Value
//                        : variant.Price;
//                    productSnapshot.PriceOriginal = variant.Price;
//                    productSnapshot.Stock = variant.Stock;
//                    productSnapshot.Attributes = variant.AttributeValues;
//                    productSnapshot.VariantId = variantId;
//                    productSnapshot.PrimaryImage = !string.IsNullOrEmpty(variant.VariantImage?.Url)
//                        ? variant.VariantImage.Url
//                        : productSnapshot.PrimaryImage;
//                    productSnapshot.Length = variant.Length ?? product.Length;
//                    productSnapshot.Width = variant.Width ?? product.Width;
//                    productSnapshot.Height = variant.Height ?? product.Height;  
//                    productSnapshot.Weight = variant.Weight ?? product.Weight;


//                }
//                return productSnapshot;
//            }
//            catch (Exception ex)
//            {

//                _logger.LogError(ex,
//       "Failed to get product info for {ProductId}. Inner exception: {Inner}",
//       productId,
//       ex.InnerException?.Message);
//                return null;
//            }
//        }
//    }
//}


using CartService.Application.DTOs;
using CartService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Common.Extensions;
using Shared.Common.Models;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace CartService.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger;
        public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler);
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<ProductSnapshotDTO>? GetProductInfoAsync(string productId, string? variantId)
        {
            try
            {
                var url = $"https://brightpa.me/api/products/{productId}/detail";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("GetProductInfoAsync API returned {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("GetProductInfoAsync raw JSON: {Json}", content);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var productResponse = JsonSerializer.Deserialize<ApiResponse<ProductDetailDto>>(content, options);
                var product = productResponse?.Data;

                if (product == null)
                {
                    _logger.LogError("GetProductInfoAsync deserialization returned null product");
                    return null;
                }

                var snapshot = new ProductSnapshotDTO
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName ?? string.Empty,
                    ShopName = product.ShopName ?? string.Empty,
                    ShopId = product.ShopId,
                    PriceOriginal = product.BasePrice,
                    PriceCurrent = product.FinalPrice,          // finalPrice cấp product
                    Stock = product.StockQuantity,
                    PrimaryImage = product.PrimaryImage?.FirstOrDefault() ?? string.Empty,
                    Length = product.Length,
                    Width = product.Width,
                    Height = product.Height,
                    Weight = product.Weight ?? 0
                };

                var hasVariants = product.Variants != null && product.Variants.Count > 0;

                // Nếu sản phẩm có variant mà không truyền variantId => theo logic cũ trả null
                if (hasVariants && string.IsNullOrWhiteSpace(variantId))
                {
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(variantId))
                {
                    var variant = product.Variants
                        ?.FirstOrDefault(v => v.VariantId.ToString().Equals(variantId, StringComparison.OrdinalIgnoreCase));

                    if (variant == null)
                    {
                        _logger.LogWarning("Variant {VariantId} not found in product {ProductId}", variantId, productId);
                        return null;
                    }

                    snapshot.VariantId = variantId;
                    snapshot.PriceOriginal = variant.Price;
                    snapshot.Stock = variant.Stock;
                    snapshot.Attributes = variant.AttributeValues;
                    snapshot.PrimaryImage = !string.IsNullOrEmpty(variant.VariantImage?.Url)
                        ? variant.VariantImage.Url
                        : snapshot.PrimaryImage;
                    snapshot.Length = variant.Length ?? product.Length;
                    snapshot.Width = variant.Width ?? product.Width;
                    snapshot.Height = variant.Height ?? product.Height;
                    snapshot.Weight = variant.Weight ?? product.Weight;

                    // Tính giá cuối cùng cho variant
                    decimal final = variant.Price;
                    if (variant.FlashSalePrice.HasValue && variant.FlashSalePrice.Value > 0)
                    {
                        var fs = variant.FlashSalePrice.Value;

                        // Trường hợp là % (ví dụ 4.29, 16.25 trong sample)
                        if (fs < 90 && fs <= 100)
                        {
                            final = Math.Round(variant.Price - (variant.Price * (fs / 100m)), 2);
                        }
                        // Trường hợp có thể là giá FlashSale tuyệt đối (ít khả năng ở payload hiện tại)
                        else if (fs < variant.Price)
                        {
                            final = fs;
                        }
                    }

                    snapshot.PriceCurrent = final;
                }

                return snapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get product info for {ProductId}. Inner: {Inner}",
                    productId,
                    ex.InnerException?.Message);
                return null;
            }
        }
    }
}