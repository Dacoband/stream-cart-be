using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShopService.Application.DTOs.Dashboard;
using ShopService.Application.Interfaces;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Shared.Common.Models;

namespace ShopService.Infrastructure.Services
{
    public class LivestreamServiceClient : ILivestreamServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LivestreamServiceClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public LivestreamServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<LivestreamServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Configure base URL
            var livestreamServiceUrl = configuration["ServiceUrls:LivestreamService"];
            if (!string.IsNullOrEmpty(livestreamServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(livestreamServiceUrl);
            }

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<LivestreamStatisticsDTO> GetLivestreamStatisticsAsync(Guid shopId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/livestreams/shop/{shopId}/statistics?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get livestream statistics for shop {ShopId}. Status: {StatusCode}",
                        shopId, response.StatusCode);
                    return new LivestreamStatisticsDTO();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LivestreamStatisticsDTO>>(json, _jsonOptions);

                return apiResponse?.Data ?? new LivestreamStatisticsDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream statistics for shop {ShopId}", shopId);
                return new LivestreamStatisticsDTO();
            }
        }
    }
}