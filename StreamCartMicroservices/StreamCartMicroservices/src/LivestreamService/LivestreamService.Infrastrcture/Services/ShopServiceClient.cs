using Livestreamservice.Application.DTOs;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Services
{
    public class ShopServiceClient : IShopServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ShopServiceClient> _logger;
        private readonly string _baseUrl;

        public ShopServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<ShopServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _baseUrl = configuration["ServiceUrls:ShopService"];
            if (string.IsNullOrEmpty(_baseUrl))
            {
                _baseUrl = "http://shop-service/api";
                _logger.LogWarning("Shop service URL not found in configuration. Using default: {BaseUrl}", _baseUrl);
            }

            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<ShopDTO> GetShopByIdAsync(Guid shopId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/shops/{shopId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ShopDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return apiResponse?.Data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Shop service to get shop with ID {ShopId}", shopId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting shop with ID {ShopId}", shopId);
                throw;
            }
        }

        public async Task<bool> IsShopMemberAsync(Guid shopId, Guid accountId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/shops/{shopId}/members/{accountId}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Shop service to check if account {AccountId} is a member of shop {ShopId}", accountId, shopId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error checking shop membership for account {AccountId} in shop {ShopId}", accountId, shopId);
                throw;
            }
        }

        public async Task<AddressOfShop> GetShopAddressAsync(Guid shopId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/shops/{shopId}/address");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<AddressOfShop>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return apiResponse?.Data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Shop service to get address for shop {ShopId}", shopId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting address for shop {ShopId}", shopId);
                throw;
            }
        }

        public async Task<bool> UpdateShopCompletionRateAsync(Guid shopId, decimal changeAmount, Guid updatedByAccountId)
        {
            try
            {
                var request = new { ShopId = shopId, ChangeAmount = changeAmount, UpdatedByAccountId = updatedByAccountId };
                var response = await _httpClient.PutAsJsonAsync($"/api/shops/{shopId}/completion-rate", request);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Shop service to update completion rate for shop {ShopId}", shopId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating completion rate for shop {ShopId}", shopId);
                throw;
            }
        }

        public async Task<bool> DoesShopExistAsync(Guid shopId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/shops/{shopId}/exists");
                return response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<bool>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Shop service to check if shop {ShopId} exists", shopId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error checking if shop {ShopId} exists", shopId);
                throw;
            }
        }

        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }
    }
}