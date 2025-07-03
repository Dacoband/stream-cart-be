using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Enums;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PaymentService.Infrastructure.Services
{
    public class OrderServiceClient : IOrderServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderServiceClient> _logger;
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

            var baseUrl = configuration["Services:OrderService"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
            }
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Forward the user's JWT token to the other service
        private void ForwardUserToken()
        {
            // Clear any existing Authorization header
            if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
            }

            // Get the current user's token and forward it
            var authHeader = _httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
            }
        }

        public async Task UpdateOrderPaymentStatusAsync(Guid orderId, PaymentStatus paymentStatus)
        {
            try
            {
                ForwardUserToken();

                var updatePaymentStatusDto = new UpdatePaymentStatusDto1
                {
                    Status = paymentStatus
                };

                var response = await _httpClient.PutAsJsonAsync(
                    $"api/orders/{orderId}/payment-status",
                    updatePaymentStatusDto);

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
                _logger.LogInformation("Getting order details for ID: {OrderId}", orderId);

                ForwardUserToken();

                var response = await _httpClient.GetAsync($"api/orders/{orderId}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", orderId);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get order {OrderId}. Status code: {StatusCode}",
                        orderId, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                // Log the exact response for debugging
                _logger.LogDebug("Order API response: {Content}", content);

                try
                {
                    // Use internal DTO for proper deserialization
                    var internalOrder = JsonSerializer.Deserialize<OrderResponseDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (internalOrder == null)
                        return null;

                    // Map to our public DTO
                    return new OrderDto
                    {
                        Id = internalOrder.Id,
                        OrderNumber = internalOrder.OrderCode,
                        UserId = internalOrder.AccountId,
                        TotalAmount = internalOrder.FinalAmount,
                        // Convert the numeric enum to our enum type
                        PaymentStatus = (PaymentStatus)internalOrder.PaymentStatus
                    };
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "JSON deserialization error: {Message}", jsonEx.Message);

                    // Attempt direct mapping from JSON document as fallback
                    try
                    {
                        var doc = JsonDocument.Parse(content);
                        var root = doc.RootElement;

                        return new OrderDto
                        {
                            Id = root.GetProperty("id").GetGuid(),
                            OrderNumber = root.GetProperty("orderCode").GetString() ?? string.Empty,
                            UserId = root.GetProperty("accountId").GetGuid(),
                            TotalAmount = root.GetProperty("finalAmount").GetDecimal(),
                            PaymentStatus = (PaymentStatus)root.GetProperty("paymentStatus").GetInt32()
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Fallback JSON parsing failed: {Message}", ex.Message);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}: {Message}", orderId, ex.Message);
                return null;
            }
        }

        // Internal DTO for handling numeric enum values from the API
        private class OrderResponseDto
        {
            public Guid Id { get; set; }
            public string OrderCode { get; set; } = string.Empty;
            public Guid AccountId { get; set; }
            public decimal FinalAmount { get; set; }
            public PaymentStatus PaymentStatus { get; set; }  // This is the key change - accepting as int
        }
    }

    public class UpdatePaymentStatusDto1
    {
        public PaymentStatus Status { get; set; }
    }
}