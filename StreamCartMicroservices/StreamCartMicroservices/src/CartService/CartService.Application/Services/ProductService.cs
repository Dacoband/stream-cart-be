using CartService.Application.DTOs;
using CartService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Common.Extensions;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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
            // Tạo handler mới cho mỗi instance (chỉ dùng cho dev)
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
            try { 
            var url = $"https://brightpa.me/api/products/{productId}/detail";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"API returned {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Raw JSON response: {content}"); // Log raw JSON

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var productResponse = JsonSerializer.Deserialize<ApiResponse<ProductDetailDto>>(content, options);
                var product = productResponse.Data;
            if (product == null)
            {
                _logger.LogError("Deserialization returned null");
                return null;
            }

            ProductSnapshotDTO productSnapshot = new ProductSnapshotDTO()
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ShopName = product.ShopName,
                    ShopId = product.ShopId,
                    PriceOriginal = product.BasePrice,
                    PriceCurrent = product.BasePrice,
                    Stock = product.StockQuantity,
                    PrimaryImage = product.PrimaryImage?.FirstOrDefault() ?? ""
,
                };
                if(product.Variants.Count > 0 && variantId.IsNullOrEmpty() && variantId.IsNullOrWhiteSpace())
                {
                    return null;
                }
                
                if (!variantId.IsNullOrEmpty() && !variantId.IsNullOrWhiteSpace())
                {
                    var variant = product.Variants.Where(x => x.VariantId.ToString() == variantId).FirstOrDefault();
                    productSnapshot.PriceCurrent = variant.Price;
                    productSnapshot.PriceOriginal = variant.Price;
                    productSnapshot.Stock = variant.Stock;
                    productSnapshot.Attributes = variant.AttributeValues;
                    productSnapshot.VariantId = variantId;
                    productSnapshot.PrimaryImage = !string.IsNullOrEmpty(variant.VariantImage?.Url)
                        ? variant.VariantImage.Url
                        : productSnapshot.PrimaryImage;
                }
                return productSnapshot;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex,
       "Failed to get product info for {ProductId}. Inner exception: {Inner}",
       productId,
       ex.InnerException?.Message);
                return null;
            }
        }
    }
}
