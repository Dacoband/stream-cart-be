using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using System.Text.Json;

namespace OrderService.Infrastructure.Clients
{
    public class LivestreamServiceClient : ILivestreamServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LivestreamServiceClient> _logger;

        public LivestreamServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<LivestreamServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var livestreamServiceUrl = configuration["ServiceUrls:LivestreamService"];
            if (!string.IsNullOrEmpty(livestreamServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(livestreamServiceUrl);
            }
        }

        public async Task<LivestreamInfoDTO?> GetLivestreamByIdAsync(Guid livestreamId)
        {
            try
            {
                _logger.LogInformation("Getting livestream {LivestreamId} from Livestream Service", livestreamId);

                var response = await _httpClient.GetAsync($"api/livestreams/{livestreamId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Livestream {LivestreamId} not found: {StatusCode}", livestreamId, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LivestreamInfoDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream {LivestreamId}", livestreamId);
                return null;
            }
        }

        public async Task<List<LivestreamInfoDTO>> GetLivestreamsByShopIdAsync(Guid shopId)
        {
            try
            {
                _logger.LogInformation("Getting livestreams for shop {ShopId}", shopId);

                var response = await _httpClient.GetAsync($"api/livestreams/shop/{shopId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get livestreams for shop {ShopId}. Status: {StatusCode}", shopId, response.StatusCode);
                    return new List<LivestreamInfoDTO>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<LivestreamInfoDTO>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data ?? new List<LivestreamInfoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestreams for shop {ShopId}", shopId);
                return new List<LivestreamInfoDTO>();
            }
        }

        public async Task<bool> DoesLivestreamExistAsync(Guid livestreamId)
        {
            try
            {
                _logger.LogInformation("Checking if livestream exists: {LivestreamId}", livestreamId);

                var response = await _httpClient.GetAsync($"api/livestreams/{livestreamId}/exists");
                if (!response.IsSuccessStatusCode)
                {
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
                _logger.LogError(ex, "Error checking livestream existence {LivestreamId}", livestreamId);
                return false;
            }
        }

        public async Task<LivestreamBasicInfoDTO?> GetLivestreamBasicInfoAsync(Guid livestreamId)
        {
            try
            {
                var livestreamInfo = await GetLivestreamByIdAsync(livestreamId);
                if (livestreamInfo == null) return null;

                return new LivestreamBasicInfoDTO
                {
                    Id = livestreamInfo.Id,
                    Title = livestreamInfo.Title,
                    ShopId = livestreamInfo.ShopId,
                    ShopName = livestreamInfo.ShopName,
                    ThumbnailUrl = livestreamInfo.ThumbnailUrl,
                    Status = livestreamInfo.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream basic info {LivestreamId}", livestreamId);
                return null;
            }
        }
    }
}