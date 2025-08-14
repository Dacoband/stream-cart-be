using ChatBoxService.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChatBoxService.Infrastructure.Services
{
    public interface ILivestreamServiceClient
    {
        /// <summary>
        /// Lấy sản phẩm trong livestream theo SKU
        /// </summary>
        Task<LivestreamProductDTO?> GetProductBySkuAsync(Guid livestreamId, string sku);

        /// <summary>
        /// Cập nhật stock của sản phẩm trong livestream
        /// </summary>
        Task<bool> UpdateProductStockAsync(Guid livestreamId, string productId, string? variantId, int newStock, string modifiedBy);

        /// <summary>
        /// Tạo StreamEvent
        /// </summary>
        Task<StreamEventResult> CreateStreamEventAsync(Guid livestreamId, Guid userId, string message, Guid livestreamProductId);

        /// <summary>
        /// Lấy thông tin livestream
        /// </summary>
        Task<LivestreamDto?> GetLivestreamByIdAsync(Guid livestreamId);
    }

    public class LivestreamServiceClient : ILivestreamServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LivestreamServiceClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LivestreamServiceClient(HttpClient httpClient, ILogger<LivestreamServiceClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        private void SetAuthorizationHeader()
        {
            try
            {
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authHeader.Replace("Bearer ", ""));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set authorization header");
            }
        }
        public async Task<LivestreamProductDTO?> GetProductBySkuAsync(Guid livestreamId, string sku)
        {
            try
            {
                SetAuthorizationHeader();
                _logger.LogInformation("Getting product by SKU {Sku} in livestream {LivestreamId}", sku, livestreamId);

                var response = await _httpClient.GetAsync($"/api/livestream-products/livestream/{livestreamId}/sku/{sku}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<LivestreamProductDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by SKU {Sku} in livestream {LivestreamId}", sku, livestreamId);
                return null;
            }
        }

        public async Task<bool> UpdateProductStockAsync(Guid livestreamId, string productId, string? variantId, int newStock, string modifiedBy)
        {
            try
            {
                // ✅ FIX: Sử dụng đúng endpoint từ LivestreamProductController
                // Endpoint: /api/livestream-products/livestream/{livestreamId}/product/{productId}/variant/{variantId}/stock
                var endpoint = $"/api/livestream-products/livestream/{livestreamId}/product/{productId}/variant/{variantId ?? "null"}/stock";

                var updateStockRequest = new
                {
                    Stock = newStock
                };

                var response = await _httpClient.PatchAsync(endpoint, JsonContent.Create(updateStockRequest));

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated stock for product {ProductId} in livestream {LivestreamId} to {Stock}",
                        productId, livestreamId, newStock);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to update stock for product {ProductId} in livestream {LivestreamId}. Status: {StatusCode}, Error: {Error}",
                        productId, livestreamId, response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for product {ProductId} in livestream {LivestreamId}", productId, livestreamId);
                return false;
            }
        }

        public async Task<StreamEventResult> CreateStreamEventAsync(Guid livestreamId, Guid userId, string message, Guid livestreamProductId)
        {
            try
            {
                SetAuthorizationHeader();
                var createEventRequest = new
                {
                    LivestreamId = livestreamId,
                   // UserId = userId,
                    LivestreamProductId = livestreamProductId,
                    EventType = "ORDER_COMMENT",
                    EvePayloadnt = message,                 
                };
                var response = await _httpClient.PostAsJsonAsync("/api/stream-events", createEventRequest);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<StreamEventDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    return new StreamEventResult
                    {
                        Id = apiResponse?.Data?.Id ?? Guid.NewGuid(),
                        Success = true,
                        Message = "Stream event created successfully"
                    };
                }

                return new StreamEventResult
                {
                    Success = false,
                    Message = "Failed to create stream event"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stream event");
                return new StreamEventResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<LivestreamDto?> GetLivestreamByIdAsync(Guid livestreamId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/livestreams/{livestreamId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<LivestreamDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream {LivestreamId}", livestreamId);
                return null;
            }
        }
    }

    // DTOs for Livestream Service
    public class LivestreamProductDTO
    {
        public Guid Id { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string? VariantId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ProductStock { get; set; }
        public Guid ShopId { get; set; }
        public string? ProductImageUrl { get; set; }
    }

    public class StreamEventResult
    {
        public Guid Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class StreamEventDto
    {
        public Guid Id { get; set; }
        public Guid LivestreamId { get; set; }
        public Guid UserId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventData { get; set; } = string.Empty;
    }

    public class LivestreamDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid ShopId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}