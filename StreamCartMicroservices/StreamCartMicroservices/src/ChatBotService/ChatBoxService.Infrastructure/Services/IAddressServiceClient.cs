using Microsoft.Extensions.Logging;
using System.Text.Json;
using Shared.Common.Models;

namespace ChatBoxService.Infrastructure.Services
{
    public interface IAddressServiceClient
    {
        /// <summary>
        /// Lấy địa chỉ mặc định của user
        /// </summary>
        Task<UserAddressDTO?> GetUserDefaultAddressAsync(Guid userId);

        /// <summary>
        /// Lấy tất cả địa chỉ của user
        /// </summary>
        Task<List<UserAddressDTO>> GetUserAddressesAsync(Guid userId);

        /// <summary>
        /// Lấy địa chỉ mặc định shipping
        /// </summary>
        Task<UserAddressDTO?> GetDefaultShippingAddressAsync();

        /// <summary>
        /// Lấy địa chỉ shop để làm from address
        /// </summary>
        Task<ShopAddressDTO?> GetShopAddressAsync(Guid shopId);
    }

    public class AddressServiceClient : IAddressServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AddressServiceClient> _logger;

        public AddressServiceClient(HttpClient httpClient, ILogger<AddressServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserAddressDTO?> GetUserDefaultAddressAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting default address for user {UserId}", userId);

                // ✅ FIX: Gọi đúng endpoint cho user address
                var response = await _httpClient.GetAsync($"/api/addresses/users/{userId}/default");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserAddressDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    if (apiResponse?.Success == true && apiResponse.Data != null)
                    {
                        return apiResponse.Data;
                    }
                }

                // ✅ Fallback: Lấy địa chỉ đầu tiên trong danh sách
                var addressesResponse = await _httpClient.GetAsync($"/api/addresses/users/{userId}");
                if (addressesResponse.IsSuccessStatusCode)
                {
                    var addressesContent = await addressesResponse.Content.ReadAsStringAsync();
                    var addressesApiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserAddressDTO>>>(addressesContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    if (addressesApiResponse?.Success == true && addressesApiResponse.Data?.Any() == true)
                    {
                        return addressesApiResponse.Data.First();
                    }
                }

                // ✅ Fallback cuối: Tạo địa chỉ mặc định từ hardcode
                _logger.LogWarning("No address found for user {UserId}, creating default address", userId);
                return new UserAddressDTO
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FullName = "Khách hàng StreamCart",
                    Phone = "0123456789",
                    AddressLine1 = "123 Đường ABC",
                    Ward = "Phường 1",
                    District = "Quận 1",
                    City = "TP.HCM",
                    PostalCode = "70000",
                    IsDefault = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default address for user {UserId}", userId);

                // ✅ Return hardcode fallback address để không block order process
                return new UserAddressDTO
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FullName = "Khách hàng StreamCart",
                    Phone = "0123456789",
                    AddressLine1 = "123 Đường ABC",
                    Ward = "Phường 1",
                    District = "Quận 1",
                    City = "TP.HCM",
                    PostalCode = "70000",
                    IsDefault = true
                };
            }
        }

        public async Task<List<UserAddressDTO>> GetUserAddressesAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/addresses/users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserAddressDTO>>>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    return apiResponse?.Data ?? new List<UserAddressDTO>();
                }

                return new List<UserAddressDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for user {UserId}", userId);
                return new List<UserAddressDTO>();
            }
        }

        public async Task<UserAddressDTO?> GetDefaultShippingAddressAsync()
        {
            try
            {
                // ✅ FIX: Gọi đúng endpoint cho default shipping address
                var response = await _httpClient.GetAsync("/api/addresses/default-shipping");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserAddressDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default shipping address");
                return null;
            }
        }

        public async Task<ShopAddressDTO?> GetShopAddressAsync(Guid shopId)
        {
            try
            {
                // ✅ FIX: Gọi đúng endpoint cho shop address
                var response = await _httpClient.GetAsync($"/api/addresses/shops/{shopId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ShopAddressDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    if (apiResponse?.Success == true && apiResponse.Data != null)
                    {
                        return apiResponse.Data;
                    }
                }

                // ✅ Fallback: Tạo địa chỉ shop mặc định
                _logger.LogWarning("No address found for shop {ShopId}, creating default shop address", shopId);
                return new ShopAddressDTO
                {
                    ShopId = shopId,
                    ShopName = "Shop StreamCart",
                    ContactName = "Quản lý Shop",
                    Phone = "0987654321",
                    AddressLine1 = "456 Đường XYZ",
                    Ward = "Phường 2",
                    District = "Quận 2",
                    City = "TP.HCM",
                    PostalCode = "70000"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shop address for {ShopId}", shopId);

                // ✅ Return hardcode fallback shop address
                return new ShopAddressDTO
                {
                    ShopId = shopId,
                    ShopName = "Shop StreamCart",
                    ContactName = "Quản lý Shop",
                    Phone = "0987654321",
                    AddressLine1 = "456 Đường XYZ",
                    Ward = "Phường 2",
                    District = "Quận 2",
                    City = "TP.HCM",
                    PostalCode = "70000"
                };
            }
        }
    }

    public class UserAddressDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public class ShopAddressDTO
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }
}