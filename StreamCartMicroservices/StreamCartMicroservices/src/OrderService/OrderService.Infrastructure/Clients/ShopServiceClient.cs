using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

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

                var response = await _httpClient.GetAsync($"/api/shops/{shopId}");
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
                var response = await _httpClient.GetAsync($"/api/shops/{shopId}/members/check/{accountId}");

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

                // Call the correct endpoint from AddressController
                var response = await _httpClient.GetAsync($"/api/addresses/shops/{shopId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(content);

                    // Extract address from response which contains a collection of addresses
                    if (json.RootElement.TryGetProperty("data", out var dataElement) &&
                        dataElement.ValueKind == JsonValueKind.Array &&
                        dataElement.GetArrayLength() > 0)
                    {
                        // Get first address (preferably a default one if indicated)
                        var addressElement = dataElement[0];

                        // Try to find default shipping address first
                        for (int i = 0; i < dataElement.GetArrayLength(); i++)
                        {
                            if (GetJsonPropertyValue(dataElement[i], "isDefaultShipping", false))
                            {
                                addressElement = dataElement[i];
                                break;
                            }
                        }

                        return new AddressOfShop
                        {
                            Name = GetJsonPropertyValue(addressElement, "recipientName", string.Empty),
                            Address = GetJsonPropertyValue(addressElement, "street", string.Empty),
                            Ward = GetJsonPropertyValue(addressElement, "ward", string.Empty),
                            District = GetJsonPropertyValue(addressElement, "district", string.Empty),
                            City = GetJsonPropertyValue(addressElement, "city", string.Empty),
                            PostalCode = GetJsonPropertyValue(addressElement, "postalCode", string.Empty),
                            PhoneNumber = GetJsonPropertyValue(addressElement, "phoneNumber", string.Empty)
                        };
                    }
                }

                // Fallback to get shop details if no address found
                var shop = await GetShopByIdAsync(shopId);
                if (shop == null)
                {
                    _logger.LogWarning("Shop {ShopId} not found", shopId);
                    return new AddressOfShop();
                }

                // Return default shop information if no address found
                return new AddressOfShop
                {
                    Name = shop.ShopName,
                    Address = "Shop Address", // Default placeholder
                    Ward = "Shop Ward",
                    District = "Shop District",
                    City = "Shop City",
                    PostalCode = "000000",
                    PhoneNumber = "0000000000"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address for shop {ShopId}", shopId);
                return new AddressOfShop();
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
    }
}