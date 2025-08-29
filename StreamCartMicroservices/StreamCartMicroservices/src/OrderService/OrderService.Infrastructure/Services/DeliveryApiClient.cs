using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.DeliveryDTOs;
using OrderService.Application.Interfaces;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Services
{
    public class DeliveryApiClient : IDeliveryApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DeliveryApiClient> _logger;
        private const string BaseUrl = "https://brightpa.me/api/deliveries";

        public DeliveryApiClient(HttpClient httpClient, ILogger<DeliveryApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DeliveryApiResponse?> GetOrderLogAsync(string trackingCode)
        {
            try
            {
                _logger.LogInformation("Getting order log for tracking code: {TrackingCode}", trackingCode);

                var response = await _httpClient.GetAsync($"{BaseUrl}/order-log/{trackingCode}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get order log for tracking code {TrackingCode}. Status: {StatusCode}",
                        trackingCode, response.StatusCode);
                    return null;
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<DeliveryApiResponse>(jsonContent, options);

                _logger.LogInformation("Successfully retrieved order log for tracking code: {TrackingCode}", trackingCode);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order log for tracking code: {TrackingCode}", trackingCode);
                return null;
            }
        }
    }
}