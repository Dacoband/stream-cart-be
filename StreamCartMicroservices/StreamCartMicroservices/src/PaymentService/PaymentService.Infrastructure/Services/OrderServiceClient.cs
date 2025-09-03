using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Enums;
using Shared.Common.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Services
{
    public class OrderServiceClient : IOrderServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderServiceClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

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
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
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
        public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus orderStatus)
        {
            try
            {
                ForwardUserToken();
                var updateOrderStatusDto = new 
                {
                    Status = orderStatus
                };
                var response = await _httpClient.PutAsJsonAsync(
                    $"api/orders/{orderId}/status",
                    updateOrderStatusDto);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {OrderId}", orderId);
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
                        PaymentStatus = (PaymentStatus)internalOrder.PaymentStatus,
                        Status = internalOrder.OrderStatus 
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

        public async Task<RefundRequestDto?> GetRefundRequestByIdAsync(Guid refundRequestId)
        {
            try
            {
                _logger.LogInformation("Getting refund request details for ID: {RefundRequestId}", refundRequestId);

              //  ForwardUserToken();

                var response = await _httpClient.GetAsync($"https://brightpa.me/api/refunds/{refundRequestId}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Refund request with ID {RefundRequestId} not found", refundRequestId);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get refund request {RefundRequestId}. Status code: {StatusCode}",
                        refundRequestId, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Refund request API response: {Content}", json);

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<RefundRequestDto>>(json, _jsonOptions);

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund request {RefundRequestId}", refundRequestId);
                return null;
            }
        }
        public async Task<bool> UpdateRefundRequestStatusAsync(Guid refundRequestId, string status)
        {
            try
            {
                _logger.LogInformation("Updating refund request {RefundRequestId} status to {Status}",
                    refundRequestId, status);

                ForwardUserToken();

                // ✅ FIX: Convert string status to enum integer value
                int statusValue = ConvertRefundStatusToInt(status);

                var updateDto = new
                {
                    RefundRequestId = refundRequestId,
                    NewStatus = statusValue // ✅ Gửi số thay vì string
                };

                var json = JsonSerializer.Serialize(updateDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("api/refunds/status", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated refund request {RefundRequestId} status to {Status} (value: {StatusValue})",
                        refundRequestId, status, statusValue);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to update refund request {RefundRequestId} status. Status: {StatusCode}, Error: {Error}",
                    refundRequestId, response.StatusCode, errorContent);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund request {RefundRequestId} status", refundRequestId);
                return false;
            }
        }
        public async Task<bool> UpdateRefundTransactionIdAsync(Guid refundRequestId, string transactionId)
        {
            try
            {
                _logger.LogInformation("Updating refund transaction ID for refund {RefundRequestId} to {TransactionId}",
                    refundRequestId, transactionId);

                ForwardUserToken();

                var updateDto = new
                {
                    RefundRequestId = refundRequestId,
                    TransactionId = transactionId
                };

                var json = JsonSerializer.Serialize(updateDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("api/refunds/transaction", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated refund transaction ID for {RefundRequestId}",
                        refundRequestId);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to update refund transaction ID. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund transaction ID for {RefundRequestId}", refundRequestId);
                return false;
            }
        }

        /// <summary>
        /// ✅ Convert string refund status to corresponding enum integer value
        /// </summary>
        private int ConvertRefundStatusToInt(string status)
        {
            return status?.ToLower() switch
            {
                "created" => 0,
                "confirmed" => 1,
                "packed" => 2,
                "ondelivery" => 3,
                "delivered" => 4,
                "completed" => 5,
                "refunded" => 6, // ✅ Đúng giá trị 6
                "rejected" => 7,
                _ => throw new ArgumentException($"Unknown refund status: {status}")
            };
        }
        // Internal DTO for handling numeric enum values from the API
        private class OrderResponseDto
        {
            public Guid Id { get; set; }
            public string OrderCode { get; set; } = string.Empty;
            public Guid AccountId { get; set; }
            public decimal FinalAmount { get; set; }
            public PaymentStatus PaymentStatus { get; set; }  // This is the key change - accepting as int
            public  OrderStatus OrderStatus { get; set; } 
        }
    }

    public class UpdatePaymentStatusDto1
    {
        public PaymentStatus Status { get; set; }
    }
}