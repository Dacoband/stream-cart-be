using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Shared.Common.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderService.Infrastructure.Clients
{
    public class LivestreamServiceClient : ILivestreamServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LivestreamServiceClient> _logger;

        public LivestreamServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<LivestreamServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var livestreamServiceUrl = configuration["ServiceUrls:LivestreamService"];
            if (!string.IsNullOrEmpty(livestreamServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(livestreamServiceUrl);
            }
        }

        public async Task<LivestreamInfoDTO?> GetLivestreamByIdAsync(Guid livestreamId)
        {
            try
            {
                _logger.LogInformation("Getting livestream {LivestreamId} from Livestream Service", livestreamId);

                var response = await _httpClient.GetAsync($"api/livestreams/{livestreamId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Livestream {LivestreamId} not found: {StatusCode}", livestreamId, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();

                // ✅ FIX: Use custom converter to handle Status field properly
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new BooleanToStringConverter() }
                };

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LivestreamInfoDTO>>(json, options);

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream {LivestreamId}", livestreamId);
                return null;
            }
        }

        public async Task<List<LivestreamInfoDTO>> GetLivestreamsByShopIdAsync(Guid shopId)
        {
            try
            {
                _logger.LogInformation("Getting livestreams for shop {ShopId}", shopId);

                var response = await _httpClient.GetAsync($"api/livestreams/shop/{shopId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get livestreams for shop {ShopId}. Status: {StatusCode}", shopId, response.StatusCode);
                    return new List<LivestreamInfoDTO>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<LivestreamInfoDTO>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data ?? new List<LivestreamInfoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestreams for shop {ShopId}", shopId);
                return new List<LivestreamInfoDTO>();
            }
        }

        public async Task<bool> DoesLivestreamExistAsync(Guid livestreamId)
        {
            try
            {
                _logger.LogInformation("Checking if livestream exists: {LivestreamId}", livestreamId);

                var response = await _httpClient.GetAsync($"api/livestreams/{livestreamId}/exists");
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<bool>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking livestream existence {LivestreamId}", livestreamId);
                return false;
            }
        }

        public async Task<LivestreamBasicInfoDTO?> GetLivestreamBasicInfoAsync(Guid livestreamId)
        {
            try
            {
                var livestreamInfo = await GetLivestreamByIdAsync(livestreamId);
                if (livestreamInfo == null) return null;

                return new LivestreamBasicInfoDTO
                {
                    Id = livestreamInfo.Id,
                    Title = livestreamInfo.Title,
                    ShopId = livestreamInfo.ShopId,
                    ShopName = livestreamInfo.ShopName,
                    ThumbnailUrl = livestreamInfo.ThumbnailUrl,
                    Status = livestreamInfo.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream basic info {LivestreamId}", livestreamId);
                return null;
            }
        }
        public async Task<bool> UpdateProductStockAsync(Guid livestreamId, string productId, string? variantId, int quantityChange, string modifiedBy)
        {
            try
            {
                _logger.LogInformation("🔄 Updating livestream product stock - LivestreamId: {LivestreamId}, ProductId: {ProductId}, VariantId: {VariantId}, Change: {Change}",
                    livestreamId, productId, variantId, quantityChange);

                var currentProduct = await GetLivestreamProductAsync(livestreamId, productId, variantId);
                if (currentProduct == null)
                {
                    _logger.LogWarning("⚠️ Livestream product not found: ProductId {ProductId}, VariantId {VariantId} in LivestreamId {LivestreamId}",
                        productId, variantId, livestreamId);
                    return false;
                }

                var newStock = currentProduct.Stock + quantityChange; 
                if (newStock < 0)
                {
                    _logger.LogWarning("⚠️ Cannot update stock to negative value. Current: {Current}, Change: {Change}",
                        currentProduct.Stock, quantityChange);
                    return false;
                }

                var requestBody = new
                {
                    stock = newStock,
                    price = currentProduct.Price 
                };

                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var baseUrl = $"api/livestream-products/livestream/{livestreamId}/product/{productId}/stock";

                var url = string.IsNullOrEmpty(variantId)
                    ? baseUrl
                    : $"{baseUrl}?variantId={variantId}";

                var response = await _httpClient.PatchAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Successfully updated livestream product stock for ProductId: {ProductId}, NewStock: {NewStock}",
                        productId, newStock);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("⚠️ Failed to update livestream product stock. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating livestream product stock for ProductId: {ProductId} in LivestreamId: {LivestreamId}",
                    productId, livestreamId);
                return false;
            }
        }
        private async Task<LivestreamProductInfo?> GetLivestreamProductAsync(Guid livestreamId, string productId, string? variantId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://brightpa.me/api/livestream-products/livestream/{livestreamId}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<LivestreamProductInfo>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Data == null)
                {
                    return null;
                }

                // Tìm sản phẩm cụ thể
                var targetVariantId = string.IsNullOrEmpty(variantId) ? string.Empty : variantId;

                var product = apiResponse.Data.FirstOrDefault(p =>
                    p.ProductId == productId &&
                    (string.IsNullOrEmpty(p.VariantId) ? string.Empty : p.VariantId) == targetVariantId);

                if (product != null)
                {
                    _logger.LogInformation("✅ Found livestream product: ProductId={ProductId}, VariantId={VariantId}, CurrentStock={Stock}",
                        productId, variantId ?? "null", product.Stock);
                }
                else
                {
                    _logger.LogWarning("⚠️ Livestream product not found: ProductId={ProductId}, VariantId={VariantId}",
                        productId, variantId ?? "null");
                }

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream product info");
                return null;
            }
        }
        public async Task<LivestreamProductPricing?> GetLivestreamProductPricingAsync(Guid livestreamId, string productId, string? variantId)
        {
            try
            {
                _logger.LogInformation("🎥 Getting livestream product pricing for ProductId {ProductId}, VariantId {VariantId} in LivestreamId {LivestreamId}",
                    productId, variantId ?? "null", livestreamId);

                // Get the livestream product from the existing method
                var livestreamProduct = await GetLivestreamProductAsync(livestreamId, productId, variantId);

                if (livestreamProduct == null)
                {
                    _logger.LogWarning("⚠️ Livestream product not found: ProductId={ProductId}, VariantId={VariantId} in LivestreamId={LivestreamId}",
                        productId, variantId ?? "null", livestreamId);
                    return null;
                }

                // Convert to LivestreamProductPricing DTO
                var pricing = new LivestreamProductPricing
                {
                    ProductId = livestreamProduct.ProductId,
                    VariantId = livestreamProduct.VariantId,
                    LivestreamPrice = livestreamProduct.Price, 
                    OriginalPrice = livestreamProduct.Price,    
                    Stock = livestreamProduct.Stock
                };

                _logger.LogInformation("✅ Found livestream product pricing: ProductId={ProductId}, LivestreamPrice={LivestreamPrice}, Stock={Stock}",
                    productId, pricing.LivestreamPrice, pricing.Stock);

                return pricing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting livestream product pricing for ProductId {ProductId} in LivestreamId {LivestreamId}",
                    productId, livestreamId);
                return null;
            }
        }
    }
    public class LivestreamProductInfo
    {
        public string ProductId { get; set; } = string.Empty;
        public string VariantId { get; set; } = string.Empty;
        public int Stock { get; set; }
        public decimal Price { get; set; }
    }
    public class BooleanToStringConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.True:
                    return "Active"; // or "true"
                case JsonTokenType.False:
                    return "Inactive"; // or "false"
                case JsonTokenType.Null:
                    return null;
                default:
                    throw new JsonException($"Cannot convert {reader.TokenType} to string");
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }

    }
}