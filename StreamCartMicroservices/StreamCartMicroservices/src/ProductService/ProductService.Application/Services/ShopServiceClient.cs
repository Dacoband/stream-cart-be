using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Products;
using ProductService.Application.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class ShopServiceClient : IShopServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ShopServiceClient> _logger;

        public ShopServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<ShopServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var shopServiceUrl = configuration["ServiceUrls:ShopService"];
            if (!string.IsNullOrEmpty(shopServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(shopServiceUrl);
            }
        }

        public async Task<bool> DoesShopExistAsync(Guid shopId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/shops/{shopId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra shop {ShopId}", shopId);
                return false;
            }
        }

        public async Task<ShopDto?> GetShopByIdAsync(Guid shopId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ShopDto>($"api/shops/{shopId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin shop {ShopId}", shopId);
                return null;
            }
        }

        public async Task<ShopDetailDto> GetShopByIdAsyncDetail(Guid shopId)
        {
            try
            {
                var shop = await _httpClient.GetFromJsonAsync<ShopDetailDto>($"https://brightpa.me/api/shops/{shopId}");
                return shop ?? new ShopDetailDto
                {
                    Id = shopId,
                    ShopName = "Unknown Shop",
                    RegistrationDate = DateTime.UtcNow,
                    CompleteRate = 0,
                    TotalReview = 0,
                    RatingAverage = 0,
                    LogoURL = string.Empty,
                    TotalProduct = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin shop {ShopId}", shopId);

                // Trả về đối tượng mặc định nếu có lỗi
                return new ShopDetailDto
                {
                    Id = shopId,
                    ShopName = "Unknown Shop",
                    RegistrationDate = DateTime.UtcNow,
                    CompleteRate = 0,
                    TotalReview = 0,
                    RatingAverage = 0,
                    LogoURL = string.Empty,
                    TotalProduct = 0
                };
            }
        }

        public async Task<bool> UpdateShopProductCountAsync(Guid shopId, int productCount)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/shops/{shopId}/product-count",
                    new { TotalProduct = productCount });

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật số lượng sản phẩm cho shop {ShopId}", shopId);
                return false;
            }
        }
    }
}