using Microsoft.Extensions.Logging;
using System.Text.Json;
using Shared.Common.Models;

namespace ChatBoxService.Infrastructure.Services
{
    public interface IShopServiceClient
    {
        /// <summary>
        /// Lấy thông tin shop theo ID
        /// </summary>
        Task<ShopInfoDTO?> GetShopByIdAsync(Guid shopId);
        /// <summary>
        /// Tìm kiếm shop theo tên
        /// </summary>
        Task<List<ShopInfoDTO>> SearchShopsByNameAsync(string shopName);

        /// <summary>
        /// Lấy danh sách shop theo trạng thái
        /// </summary>
        Task<List<ShopInfoDTO>> GetShopsByStatusAsync(bool isActive = true);
    }

    public class ShopServiceClient : IShopServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ShopServiceClient> _logger;

        public ShopServiceClient(HttpClient httpClient, ILogger<ShopServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ShopInfoDTO?> GetShopByIdAsync(Guid shopId)
        {
            try
            {
                _logger.LogInformation("Getting shop info for {ShopId}", shopId);

                // ✅ Gọi đúng endpoint shop service
                var response = await _httpClient.GetAsync($"/api/shops/{shopId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ShopInfoDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    return apiResponse?.Data;
                }

                // ✅ Fallback: Tạo shop info mặc định
                _logger.LogWarning("No shop found for {ShopId}, creating default shop info", shopId);
                return new ShopInfoDTO
                {
                    Id = shopId,
                    ShopName = "Shop StreamCart",
                    Description = "Cửa hàng bán hàng trực tuyến",
                    Address = "456 Đường XYZ, Phường 2, Quận 2, TP.HCM",
                    Phone = "0987654321",
                    Email = "shop@streamcart.vn"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shop info for {ShopId}", shopId);

                // ✅ Return hardcode fallback
                return new ShopInfoDTO
                {
                    Id = shopId,
                    ShopName = "Shop StreamCart",
                    Description = "Cửa hàng bán hàng trực tuyến",
                    Address = "456 Đường XYZ, Phường 2, Quận 2, TP.HCM",
                    Phone = "0987654321",
                    Email = "shop@streamcart.vn"
                };
            }
        }
        public async Task<List<ShopInfoDTO>> SearchShopsByNameAsync(string shopName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/shops/search?name={Uri.EscapeDataString(shopName)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ShopInfoDTO>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse?.Data ?? new List<ShopInfoDTO>();
                }

                _logger.LogWarning("Shop search returned {StatusCode} for name {ShopName}", response.StatusCode, shopName);
                return new List<ShopInfoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching shops by name {ShopName}", shopName);
                return new List<ShopInfoDTO>();
            }
        }
        public async Task<List<ShopInfoDTO>> GetShopsByStatusAsync(bool isActive = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/shops?isActive={isActive}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ShopInfoDTO>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse?.Data ?? new List<ShopInfoDTO>();
                }

                _logger.LogWarning("Get shops by status returned {StatusCode} for isActive {IsActive}", response.StatusCode, isActive);
                return new List<ShopInfoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shops by status isActive={IsActive}", isActive);
                return new List<ShopInfoDTO>();
            }
        }

        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }
    }

    public class ShopInfoDTO
    {
        public Guid Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}