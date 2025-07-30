using ChatBoxService.Application.DTOs.ShopDto;
using ChatBoxService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatBoxService.Infrastructure.Services
{
    public class ShopServiceClient : IShopServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ShopServiceClient> _logger;

        public ShopServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<ShopServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Cấu hình base URL từ configuration
            var shopServiceUrl = configuration["ServiceUrls:ShopService"];
            if (!string.IsNullOrEmpty(shopServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(shopServiceUrl);
            }
        }

        public async Task<ShopDto?> GetShopByIdAsync(Guid shopId)
        {
            try
            {
                _logger.LogInformation("Getting shop {ShopId} from Shop Service", shopId);

                var response = await _httpClient.GetAsync($"api/shops/{shopId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Shop {ShopId} not found: {StatusCode}", shopId, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ShopDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shop {ShopId}", shopId);
                return null;
            }
        }

        public async Task<bool> DoesShopExistAsync(Guid shopId)
        {
            try
            {
                _logger.LogInformation("Checking if shop exists: {ShopId}", shopId);

                var response = await _httpClient.GetAsync($"api/shops/{shopId}/exists");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to check shop existence {ShopId}. Status: {StatusCode}", shopId, response.StatusCode);
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
                _logger.LogError(ex, "Error checking shop existence {ShopId}", shopId);
                return false;
            }
        }

        public async Task<bool> IsShopActiveAsync(Guid shopId)
        {
            try
            {
                _logger.LogInformation("Checking if shop is active: {ShopId}", shopId);

                var response = await _httpClient.GetAsync($"api/shops/{shopId}/active");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to check shop active status {ShopId}. Status: {StatusCode}", shopId, response.StatusCode);
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
                _logger.LogError(ex, "Error checking shop active status {ShopId}", shopId);
                return false;
            }
        }

        public async Task<List<ShopDto>> GetShopsByStatusAsync(bool isActive)
        {
            try
            {
                _logger.LogInformation("Getting shops by status: {IsActive}", isActive);

                var response = await _httpClient.GetAsync($"api/shops/status/{isActive}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get shops by status {IsActive}. Status: {StatusCode}", isActive, response.StatusCode);
                    return new List<ShopDto>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ShopDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data ?? new List<ShopDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shops by status {IsActive}", isActive);
                return new List<ShopDto>();
            }
        }

        public async Task<List<ShopDto>> SearchShopsByNameAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching shops by name: {SearchTerm}", searchTerm);

                var response = await _httpClient.GetAsync($"api/shops/search?q={Uri.EscapeDataString(searchTerm)}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to search shops by name {SearchTerm}. Status: {StatusCode}", searchTerm, response.StatusCode);
                    return new List<ShopDto>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ShopDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data ?? new List<ShopDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching shops by name {SearchTerm}", searchTerm);
                return new List<ShopDto>();
            }
        }
    }
}