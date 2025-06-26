using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PaymentService.Domain.Enums;

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

        public async Task UpdateOrderPaymentStatusAsync(Guid orderId, PaymentStatus paymentStatus)
        {
            try
            {
                // Create the DTO that matches what OrderController expects
                var updatePaymentStatusDto = new UpdatePaymentStatusDto1
                {
                    Status = paymentStatus
                };

                var response = await _httpClient.PutAsJsonAsync($"api/orders/{orderId}/payment-status", updatePaymentStatusDto);
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

    // Add this DTO to match the OrderController's expected input
    public class UpdatePaymentStatusDto1
    {
        public PaymentStatus Status { get; set; }
    }
}