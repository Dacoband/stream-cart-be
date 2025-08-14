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