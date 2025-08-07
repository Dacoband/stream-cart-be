using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShopService.Application.DTOs.Dashboard;
using ShopService.Application.Interfaces;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Shared.Common.Models;
using Microsoft.AspNetCore.Http;

namespace ShopService.Infrastructure.Services
{
    public class OrderServiceClient : IOrderServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderServiceClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderServiceClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<OrderServiceClient> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            // Configure base URL
            var orderServiceUrl = configuration["ServiceUrls:OrderService"];
            if (!string.IsNullOrEmpty(orderServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(orderServiceUrl);
            }

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Forward the user's JWT token to the other service
        private void ForwardUserToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", authHeader.ToString());
            }
        }

        public async Task<OrderStatisticsDTO> GetOrderStatisticsAsync(Guid shopId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                ForwardUserToken();
                var response = await _httpClient.GetAsync($"api/orders/shop/{shopId}/statistics?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get order statistics for shop {ShopId}. Status: {StatusCode}",
                        shopId, response.StatusCode);
                    return new OrderStatisticsDTO();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<OrderStatisticsDTO>>(json, _jsonOptions);

                return apiResponse?.Data ?? new OrderStatisticsDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics for shop {ShopId}", shopId);
                return new OrderStatisticsDTO();
            }
        }

        public async Task<TopProductsDTO> GetTopSellingProductsAsync(Guid shopId, DateTime fromDate, DateTime toDate, int limit = 5)
        {
            try
            {
                ForwardUserToken();
                var response = await _httpClient.GetAsync($"api/orders/shop/{shopId}/top-products?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}&limit={limit}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get top selling products for shop {ShopId}. Status: {StatusCode}",
                        shopId, response.StatusCode);
                    return new TopProductsDTO();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<TopProductsDTO>>(json, _jsonOptions);

                return apiResponse?.Data ?? new TopProductsDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top selling products for shop {ShopId}", shopId);
                return new TopProductsDTO();
            }
        }

        public async Task<CustomerStatisticsDTO> GetCustomerStatisticsAsync(Guid shopId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                ForwardUserToken();
                var response = await _httpClient.GetAsync($"api/orders/shop/{shopId}/customer-statistics?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get customer statistics for shop {ShopId}. Status: {StatusCode}",
                        shopId, response.StatusCode);
                    return new CustomerStatisticsDTO();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<CustomerStatisticsDTO>>(json, _jsonOptions);

                return apiResponse?.Data ?? new CustomerStatisticsDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer statistics for shop {ShopId}", shopId);
                return new CustomerStatisticsDTO();
            }
        }

        public async Task<OrderTimeSeriesDTO> GetOrderTimeSeriesAsync(Guid shopId, DateTime fromDate, DateTime toDate, string period)
        {
            try
            {
                ForwardUserToken();
                var response = await _httpClient.GetAsync($"api/orders/shop/{shopId}/time-series?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}&period={period}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get order time series for shop {ShopId}. Status: {StatusCode}",
                        shopId, response.StatusCode);
                    return new OrderTimeSeriesDTO();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<OrderTimeSeriesDTO>>(json, _jsonOptions);

                return apiResponse?.Data ?? new OrderTimeSeriesDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order time series for shop {ShopId} with period {Period}", shopId, period);
                return new OrderTimeSeriesDTO();
            }
        }

        public async Task<LivestreamOrdersDTO> GetLivestreamOrdersAsync(Guid shopId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                ForwardUserToken();
                var fromDateParam = fromDate.HasValue ? $"fromDate={fromDate.Value:yyyy-MM-dd}" : "";
                var toDateParam = toDate.HasValue ? $"toDate={toDate.Value:yyyy-MM-dd}" : "";
                var separator = !string.IsNullOrEmpty(fromDateParam) && !string.IsNullOrEmpty(toDateParam) ? "&" : "";
                var queryString = !string.IsNullOrEmpty(fromDateParam) || !string.IsNullOrEmpty(toDateParam) ? "?" : "";

                var response = await _httpClient.GetAsync($"api/orders/shop/{shopId}/livestream-orders{queryString}{fromDateParam}{separator}{toDateParam}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get livestream orders for shop {ShopId}. Status: {StatusCode}",
                        shopId, response.StatusCode);
                    return new LivestreamOrdersDTO();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LivestreamOrdersDTO>>(json, _jsonOptions);

                return apiResponse?.Data ?? new LivestreamOrdersDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream orders for shop {ShopId}", shopId);
                return new LivestreamOrdersDTO();
            }
        }
    }
}