using Appwrite;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Shared.Common.Models;
using Shared.Common.Settings;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Clients
{
    /// <summary>
    /// Implementation of the IShopServiceClient interface
    /// </summary>
    public class ShopServiceClient : IShopServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ShopServiceClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Creates a new instance of ShopServiceClient
        /// </summary>
        /// <param name="httpClient">HTTP client</param>
        /// <param name="logger">Logger</param>
        public ShopServiceClient(HttpClient httpClient, ILogger<ShopServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Gets shop details by ID
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>Shop details</returns>
        public async Task<ShopDto> GetShopByIdAsync(Guid shopId)
        {
            try
            {
                _logger.LogInformation("Getting shop details for ID: {ShopId}", shopId);

                var response = await _httpClient.GetAsync($"https://brightpa.me/api/shops/{shopId}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Shop with ID {ShopId} not found", shopId);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                // Deserialize and map to our local ShopDto
                var json = JsonDocument.Parse(content);

                return new ShopDto
                {
                    Id = shopId,
                    ShopName = GetJsonPropertyValue(json.RootElement, "shopName", string.Empty),
                    Description = GetJsonPropertyValue(json.RootElement, "description", string.Empty),
                    LogoURL = GetJsonPropertyValue(json.RootElement, "logoURL", string.Empty),
                    CoverImageURL = GetJsonPropertyValue(json.RootElement, "coverImageURL", string.Empty),
                    AccountId = GetJsonPropertyValue(json.RootElement, "accountId", Guid.Empty),
                    Status = GetJsonPropertyValue(json.RootElement, "status", false),
                    ApprovalStatus = GetJsonPropertyValue(json.RootElement, "approvalStatus", string.Empty)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shop details for ID: {ShopId}", shopId);
                throw;
            }
        }

        /// <summary>
        /// Checks if a shop is active
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>True if the shop is active, otherwise false</returns>
        public async Task<bool> IsShopActiveAsync(Guid shopId)
        {
            try
            {
                var shop = await GetShopByIdAsync(shopId);
                return shop?.Status ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if shop is active for ID: {ShopId}", shopId);
                return false;
            }
        }
        public async Task<bool> IsShopMemberAsync(Guid shopId, Guid accountId)
        {
            try
            {
                _logger.LogInformation("Checking if account {AccountId} is a member of shop {ShopId}", accountId, shopId);

                // Call the Shop service API to check membership
                var response = await _httpClient.GetAsync($"https://brightpa.me/api/shops/{shopId}/members/check/{accountId}");

                // If the response is successful, the account is a member
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(content);

                    // Extract the membership status from the response
                    return GetJsonPropertyValue(json.RootElement, "isMember", false);
                }

                // If we get a 404, the account is not a member
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }

                // For any other error, log it and return false
                _logger.LogWarning("Unexpected response checking shop membership. StatusCode: {StatusCode}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if account {AccountId} is a member of shop {ShopId}", accountId, shopId);
                return false;
            }
        }

        public async Task<AddressOfShop> GetShopAddressAsync(Guid shopId)
        {
            try
            {
                _logger.LogInformation("Getting address for shop {ShopId}", shopId);
                var url = $"https://brightpa.me/api/addresses/shops/{shopId}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await _httpClient.SendAsync(request);


                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var wrapper = JsonSerializer.Deserialize<ApiResponse< IEnumerable<AddressOfShop>>>(content, options);

                    if (wrapper?.Data != null && wrapper.Data.Any())
                    {
                        // Ưu tiên địa chỉ mặc định nếu có
                        var defaultAddress = wrapper.Data.FirstOrDefault(x => x.IsDefaultShipping) ?? wrapper.Data.First();

                        return defaultAddress;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address for shop {ShopId}", shopId);
                return null;
            }
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
        public async Task<bool> UpdateShopCompletionRateAsync(Guid shopId, decimal changeAmount, Guid updatedByAccountId)
        {
            try
            {
                var request = new
                {
                    RateChange = changeAmount,
                    UpdatedByAccountId = updatedByAccountId
                };

                var response = await _httpClient.PutAsJsonAsync($"https://brightpa.me/api/shops/{shopId}/completion-rate", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật tỷ lệ hoàn thành cho shop {ShopId}", shopId);
                return false;
            }
        }
        public async Task<bool> DoesShopExistAsync(Guid shopId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://brightpa.me/api/shops/{shopId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if shop {ShopId} exists", shopId);
                return false;
            }
        }
        public async Task<bool> UpdateShopRatingAsync(Guid shopId, decimal newRating, string? modifier = null)
        {
            try
            {
                var requestData = new
                {
                    Rating = newRating,
                    Modifier = modifier ?? "OrderService"
                };

                var response = await _httpClient.PutAsJsonAsync($"api/shops/{shopId}/rating", requestData);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật rating cho shop {ShopId}", shopId);
                return false;
            }
        }
    }
}