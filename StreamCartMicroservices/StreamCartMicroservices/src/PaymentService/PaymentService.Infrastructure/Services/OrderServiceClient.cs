using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Services
{
    public class OrderServiceClient : IOrderServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderServiceClient> _logger;

        public OrderServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<OrderServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Configure base URL from configuration
            var orderServiceUrl = configuration["ServiceUrls:OrderService"];
            if (!string.IsNullOrEmpty(orderServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(orderServiceUrl);
            }
        }

        public async Task UpdateOrderPaymentStatusAsync(Guid orderId, string paymentStatus)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/orders/{orderId}/payment-status", new
                {
                    Status = paymentStatus
                });

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<OrderDto>($"api/orders/{orderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                return null;
            }
        }
    }
}