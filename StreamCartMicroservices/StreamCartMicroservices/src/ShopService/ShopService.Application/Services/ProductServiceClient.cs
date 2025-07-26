using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShopService.Application.Services
{
    public class ProductServiceClient : IProductServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductServiceClient> _logger;

        public ProductServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<ProductServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Cấu hình base URL từ configuration (giống AccountServiceClient)
            var productServiceUrl = configuration["ServiceUrls:ProductService"];
            if (!string.IsNullOrEmpty(productServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(productServiceUrl);
            }
        }

        public async Task<List<ProductDto>> GetProductsByShopIdAsync(Guid shopId, bool activeOnly = true)
        {
            try
            {
                _logger.LogInformation("Getting products for shop {ShopId}, activeOnly: {ActiveOnly}", shopId, activeOnly);

                var response = await _httpClient.GetAsync($"api/products/shop/{shopId}?activeOnly={activeOnly}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get products for shop {ShopId}. Status: {StatusCode}",
                        shopId, response.StatusCode);
                    return new List<ProductDto>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data ?? new List<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products for shop {ShopId}", shopId);
                return new List<ProductDto>();
            }
        }

        public async Task<int> GetProductCountByShopIdAsync(Guid shopId, bool activeOnly = true)
        {
            try
            {
                _logger.LogInformation("Getting product count for shop {ShopId}, activeOnly: {ActiveOnly}", shopId, activeOnly);

                var response = await _httpClient.GetAsync($"api/products/shop/{shopId}/count?activeOnly={activeOnly}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get product count for shop {ShopId}. Status: {StatusCode}",
                        shopId, response.StatusCode);
                    return 0;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<int>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product count for shop {ShopId}", shopId);
                return 0;
            }
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
        {
            try
            {
                _logger.LogInformation("Getting product by ID: {ProductId}", productId);

                var response = await _httpClient.GetAsync($"api/products/{productId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get product {ProductId}. Status: {StatusCode}",
                        productId, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", productId);
                return null;
            }
        }

        public async Task<bool> DoesProductExistAsync(Guid productId)
        {
            try
            {
                _logger.LogInformation("Checking if product exists: {ProductId}", productId);

                var response = await _httpClient.GetAsync($"api/products/{productId}/exists");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to check product existence {ProductId}. Status: {StatusCode}",
                        productId, response.StatusCode);
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
                _logger.LogError(ex, "Error checking product existence {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> IsProductOwnedByShopAsync(Guid productId, Guid shopId)
        {
            try
            {
                _logger.LogInformation("Checking if product {ProductId} is owned by shop {ShopId}", productId, shopId);

                var response = await _httpClient.GetAsync($"api/products/{productId}/shop/{shopId}/owned");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to check product ownership {ProductId} by shop {ShopId}. Status: {StatusCode}",
                        productId, shopId, response.StatusCode);
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
                _logger.LogError(ex, "Error checking product ownership {ProductId} by shop {ShopId}", productId, shopId);
                return false;
            }
        }

        public async Task<bool> UpdateProductStockAsync(Guid productId, int quantityChange)
        {
            try
            {
                _logger.LogInformation("Updating product stock {ProductId} by {QuantityChange}", productId, quantityChange);

                var requestBody = new { QuantityChange = quantityChange };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/products/{productId}/stock", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to update product stock {ProductId}. Status: {StatusCode}",
                        productId, response.StatusCode);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product stock {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> UpdateProductStatusAsync(Guid productId, bool isActive)
        {
            try
            {
                _logger.LogInformation("Updating product status {ProductId} to {IsActive}", productId, isActive);
                var requestBody = isActive; 
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/products/{productId}/status", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to update product status {ProductId}. Status: {StatusCode}",
                        productId, response.StatusCode);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product status {ProductId}", productId);
                return false;
            }
        }
    }
}