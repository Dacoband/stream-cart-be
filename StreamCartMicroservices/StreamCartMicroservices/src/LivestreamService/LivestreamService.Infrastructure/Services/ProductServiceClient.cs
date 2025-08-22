using LivestreamService.Application.DTOs;
using LivestreamService.Application.DTOs.LiveStreamProduct;
using LivestreamService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Services
{
    public class ProductServiceClient : IProductServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductServiceClient> _logger;
        private readonly IConfiguration _configuration;

        public ProductServiceClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ProductServiceClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Set base address if not already set
            if (_httpClient.BaseAddress == null)
            {
                var baseUrl = configuration["ServiceUrls:ProductService"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    _httpClient.BaseAddress = new Uri(baseUrl);
                }
            }
        }
        public async Task<string?> GetCombinationStringByVariantIdAsyncs(Guid variantId)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"https://brightpa.me/api/product-combinations/{variantId}");
                var content = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GetCombinationString failed: {Status} {Body}", resp.StatusCode, content);
                    return null;
                }

                var api = JsonSerializer.Deserialize<ApiResponse<List<ProductCombinationDto>>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (api?.Success == true && api.Data != null && api.Data.Any())
                {
                    // Ghép chuỗi: "Màu Đen , Model 509"
                    var parts = api.Data
                        .Select(d => $"{d.AttributeName} {d.ValueName}")
                        .ToArray();
                    return string.Join(" , ", parts);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling product-combinations for variant {VariantId}", variantId);
                return null;
            }
        }

        public async Task<ProductDTO?> GetProductByIdAsync(string productId)
        {
            try
            {
                _logger.LogInformation("Getting product by ID: {ProductId}", productId);

                var response = await _httpClient.GetAsync($"https://brightpa.me/api/products/{productId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get product {ProductId}. Status: {StatusCode}",
                        productId, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by ID: {ProductId}", productId);
                return null;
            }
        }

        public async Task<ProductVariantDTO> GetProductVariantAsync(string productId, string variantId)
        {
            try
            {
                _logger.LogInformation("Getting product variant: ProductId={ProductId}, VariantId={VariantId}",
                    productId, variantId);

                var response = await _httpClient.GetAsync($"https://brightpa.me/api/product-variants/{variantId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get product variant {ProductId}/{VariantId}. Status: {StatusCode}",
                        productId, variantId, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductVariantDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product variant: ProductId={ProductId}, VariantId={VariantId}",
                    productId, variantId);
                return null;
            }
        }

        public async Task<bool> IsProductOwnedByShopAsync(string productId, Guid shopId)
        {
            try
            {
                _logger.LogInformation("Checking if product {ProductId} is owned by shop {ShopId}", productId, shopId);

                // Get product details first
                var product = await GetProductByIdAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found", productId);
                    return false;
                }

                // Check if the product belongs to the specified shop
                var isOwned = product.ShopId == shopId;
                _logger.LogInformation("Product {ProductId} ownership check: {IsOwned}", productId, isOwned);

                return isOwned;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product ownership: ProductId={ProductId}, ShopId={ShopId}",
                    productId, shopId);
                return false;
            }
        }

        // ✅ BỔ SUNG: Lấy thông tin FlashSale theo ID
        public async Task<FlashSaleDTO> GetFlashSaleByIdAsync(Guid flashSaleId)
        {
            try
            {
                _logger.LogInformation("Getting FlashSale by ID: {FlashSaleId}", flashSaleId);

                var response = await _httpClient.GetAsync($"api/flashsales/{flashSaleId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get FlashSale {FlashSaleId}. Status: {StatusCode}",
                        flashSaleId, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<DetailFlashSaleDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Data == null)
                {
                    _logger.LogWarning("FlashSale {FlashSaleId} not found or API returned null data", flashSaleId);
                    return null;
                }

                // Map từ DetailFlashSaleDTO sang FlashSaleDTO
                return new FlashSaleDTO
                {
                    Id = apiResponse.Data.Id,
                    ProductId = apiResponse.Data.ProductId,
                    VariantId = apiResponse.Data.VariantId,
                    FlashSalePrice = apiResponse.Data.FlashSalePrice,
                    StartTime = apiResponse.Data.StartTime,
                    EndTime = apiResponse.Data.EndTime,
                    IsActive = apiResponse.Data.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting FlashSale by ID: {FlashSaleId}", flashSaleId);
                return null;
            }
        }

        // ✅ BỔ SUNG: Kiểm tra FlashSale có hợp lệ cho sản phẩm/variant không
        public async Task<bool> IsFlashSaleValidForProductAsync(Guid flashSaleId, string productId, string? variantId = null)
        {
            try
            {
                _logger.LogInformation("Validating FlashSale {FlashSaleId} for Product {ProductId}, Variant {VariantId}",
                    flashSaleId, productId, variantId);

                var flashSale = await GetFlashSaleByIdAsync(flashSaleId);
                if (flashSale == null)
                {
                    _logger.LogWarning("FlashSale {FlashSaleId} not found", flashSaleId);
                    return false;
                }

                // Kiểm tra FlashSale có áp dụng cho sản phẩm này không
                if (flashSale.ProductId.ToString() != productId)
                {
                    _logger.LogWarning("FlashSale {FlashSaleId} does not apply to product {ProductId}",
                        flashSaleId, productId);
                    return false;
                }

                // Kiểm tra variant nếu có
                if (!string.IsNullOrEmpty(variantId))
                {
                    if (flashSale.VariantId?.ToString() != variantId)
                    {
                        _logger.LogWarning("FlashSale {FlashSaleId} does not apply to variant {VariantId}",
                            flashSaleId, variantId);
                        return false;
                    }
                }
                else
                {
                    // Nếu không có variantId, FlashSale phải áp dụng cho sản phẩm gốc (không có variant)
                    if (flashSale.VariantId.HasValue)
                    {
                        _logger.LogWarning("FlashSale {FlashSaleId} is for variant but no variant specified",
                            flashSaleId);
                        return false;
                    }
                }

                // Kiểm tra FlashSale còn hiệu lực không
                var now = DateTime.UtcNow;
                if (flashSale.StartTime > now || flashSale.EndTime < now)
                {
                    _logger.LogWarning("FlashSale {FlashSaleId} is not active. Start: {Start}, End: {End}, Now: {Now}",
                        flashSaleId, flashSale.StartTime, flashSale.EndTime, now);
                    return false;
                }

                // Kiểm tra trạng thái active
                if (!flashSale.IsActive)
                {
                    _logger.LogWarning("FlashSale {FlashSaleId} is not active", flashSaleId);
                    return false;
                }

                _logger.LogInformation("FlashSale {FlashSaleId} is valid for product {ProductId}",
                    flashSaleId, productId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating FlashSale {FlashSaleId} for product {ProductId}",
                    flashSaleId, productId);
                return false;
            }
        }

        // Helper methods for the methods that were previously implemented with Guid parameters
        public async Task<ProductDTO?> GetProductByIdAsync(Guid productId)
        {
            return await GetProductByIdAsync(productId.ToString());
        }

        public async Task<List<ProductDTO>> GetProductsByShopIdAsync(Guid shopId)
        {
            try
            {
                _logger.LogInformation("Getting products by shop ID: {ShopId}", shopId);

                var response = await _httpClient.GetAsync($"api/products/shop/{shopId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get products for shop {ShopId}. Status: {StatusCode}",
                        shopId, response.StatusCode);
                    return new List<ProductDTO>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductDTO>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data ?? new List<ProductDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by shop ID: {ShopId}", shopId);
                return new List<ProductDTO>();
            }
        }

        public async Task<List<ProductDTO>> GetProductsWithFlashSaleAsync()
        {
            try
            {
                _logger.LogInformation("Getting products with flash sale");

                var response = await _httpClient.GetAsync("api/products/flash-sales");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get products with flash sale. Status: {StatusCode}",
                        response.StatusCode);
                    return new List<ProductDTO>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductDTO>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data ?? new List<ProductDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products with flash sale");
                return new List<ProductDTO>();
            }
        }

        public async Task<bool> UpdateProductStatusAsync(Guid productId, bool isActive)
        {
            try
            {
                _logger.LogInformation("Updating product status: {ProductId} to {IsActive}", productId, isActive);

                var requestContent = new StringContent(
                    JsonSerializer.Serialize(isActive),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PatchAsync($"api/products/{productId}/status", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated product status: {ProductId}", productId);
                    return true;
                }

                _logger.LogWarning("Failed to update product status {ProductId}. Status: {StatusCode}",
                    productId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product status: {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> CheckProductExistsAsync(Guid productId)
        {
            try
            {
                _logger.LogInformation("Checking if product exists: {ProductId}", productId);

                var response = await _httpClient.GetAsync($"api/products/{productId}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product existence: {ProductId}", productId);
                return false;
            }
        }
        public async Task<string?> GetCombinationStringByVariantIdAsync(Guid variantId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://brightpa.me/api/product-combinations/{variantId}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductCombinationDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Data == null || !apiResponse.Data.Any())
                {
                    return null;
                }

                // Ghép AttributeName + ValueName thành chuỗi "Color Red + Size M"
                var combinationString = string.Join(" + ",
                    apiResponse.Data.Select(c => $"{c.AttributeName} {c.ValueName}")
                );

                return combinationString;
            }
            catch (Exception ex)
            {
                // Có thể log lỗi ở đây
                return null;
            }
        }
        public async Task<ProductVariantWithDimensionsDTO?> GetProductVariantWithDimensionsAsync(string productId, string variantId)
        {
            try
            {
                _logger.LogInformation("Getting product variant with dimensions: ProductId={ProductId}, VariantId={VariantId}",
                    productId, variantId);

                var response = await _httpClient.GetAsync($"https://brightpa.me/api/product-variants/{variantId}/dimensions");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get product variant with dimensions {ProductId}/{VariantId}. Status: {StatusCode}",
                        productId, variantId, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();

                // Deserialize trực tiếp sang DTO mới (API có thể trả dư fields)
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductVariantWithDimensionsDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product variant with dimensions: ProductId={ProductId}, VariantId={VariantId}",
                    productId, variantId);
                return null;
            }
        }

    }

    // Helper class for API responses
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    // ✅ BỔ SUNG: DTO cho FlashSale từ Product API
    public class DetailFlashSaleDTO
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public decimal FlashSalePrice { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantitySold { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
    }
}