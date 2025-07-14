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
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IConfiguration _configuration;

        public ShopServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<ShopServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Set the base address for the HTTP client
            var shopServiceUrl = _configuration["ServiceUrls:ShopService"];
            if (!string.IsNullOrEmpty(shopServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(shopServiceUrl);
                _logger.LogInformation("ShopServiceClient configured with base URL: {BaseUrl}", shopServiceUrl);
            }
            else
            {
                _logger.LogWarning("ServiceUrls:ShopService is not configured. HTTP requests may fail.");
            }
        }

        public async Task<ShopDTO> GetShopByIdAsync(Guid shopId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/shops/{shopId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                // Deserialize and map to our local ShopDto
                var json = JsonDocument.Parse(content);

                return new ShopDTO
                {
                    Id = shopId,
                    ShopName = GetJsonPropertyValue(json.RootElement, "shopName", string.Empty),
                    Description = GetJsonPropertyValue(json.RootElement, "description", string.Empty),
                    LogoURL = GetJsonPropertyValue(json.RootElement, "logoURL", string.Empty),
                    CoverImageURL = GetJsonPropertyValue(json.RootElement, "coverImageURL", string.Empty),
                    //AccountId = GetJsonPropertyValue(json.RootElement, "accountId", Guid.Empty),
                    Status = GetJsonPropertyValue(json.RootElement, "status", false),
                    ApprovalStatus = GetJsonPropertyValue(json.RootElement, "approvalStatus", string.Empty)
                };
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
        private T GetJsonPropertyValue<T>(JsonElement element, string propertyName, T defaultValue)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property))
            {
                try
                {
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)property.GetString()!;
                    }
                    else if (typeof(T) == typeof(bool))
                    {
                        return (T)(object)property.GetBoolean();
                    }
                    else if (typeof(T) == typeof(int))
                    {
                        return (T)(object)property.GetInt32();
                    }
                    else if (typeof(T) == typeof(decimal))
                    {
                        return (T)(object)property.GetDecimal();
                    }
                    else if (typeof(T) == typeof(Guid))
                    {
                        return (T)(object)Guid.Parse(property.GetString()!);
                    }
                    else if (typeof(T) == typeof(DateTime))
                    {
                        return (T)(object)property.GetDateTime();
                    }
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }
}