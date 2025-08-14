using ChatBoxService.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChatBoxService.Infrastructure.Services
{
    public interface IOrderServiceClient
    {
        /// <summary>
        /// Tạo multiple orders cho livestream
        /// </summary>
        Task<CreateMultiOrderResult> CreateMultiOrderAsync(CreateMultiOrderRequest request);

        /// <summary>
        /// Lấy thông tin order theo ID
        /// </summary>
        Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId);

        /// <summary>
        /// Hủy order
        /// </summary>
        Task<bool> CancelOrderAsync(Guid orderId, string cancelReason, string cancelledBy = "system");

        /// <summary>
        /// Kiểm tra trạng thái payment của order
        /// </summary>
        Task<string?> GetOrderPaymentStatusAsync(Guid orderId);
    }

    public class OrderServiceClient : IOrderServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderServiceClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderServiceClient(HttpClient httpClient, ILogger<OrderServiceClient> logger, IHttpContextAccessor httpContextAccessor)
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
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set authorization header");
            }
        }
        public async Task<CreateMultiOrderResult> CreateMultiOrderAsync(CreateMultiOrderRequest request)
        {
            try
            {
                SetAuthorizationHeader();
                _logger.LogInformation("Creating multi order for account {AccountId}", request.AccountId);

                var response = await _httpClient.PostAsJsonAsync("/api/orders/multi", request);
                var responseContent = await response.Content.ReadAsStringAsync();


                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<OrderDto>>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true && apiResponse.Data?.Any() == true)
                    {
                        return new CreateMultiOrderResult
                        {
                            Success = true,
                            OrderId = apiResponse.Data.First().Id,
                            OrderCode = apiResponse.Data.First().OrderCode,
                            Message = "Tạo đơn hàng thành công"
                        };
                    }
                    else
                    {
                        _logger.LogWarning("API returned success status but response indicates failure: {Response}", responseContent);
                        return new CreateMultiOrderResult
                        {
                            Success = false,
                            Message = apiResponse?.Message ?? "Không thể tạo đơn hàng"
                        };
                    }
                }
                else
                {
                    _logger.LogError("Failed to create multi order. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, responseContent);
                }

                return new CreateMultiOrderResult
                {
                    Success = false,
                    Message = "Không thể tạo đơn hàng"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating multi order");
                return new CreateMultiOrderResult
                {
                    Success = false,
                    Message = "Lỗi hệ thống khi tạo đơn hàng"
                };
            }
        }

        public async Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/orders/{orderId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<OrderDetailDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                return null;
            }
        }

        public async Task<bool> CancelOrderAsync(Guid orderId, string cancelReason, string cancelledBy = "system")
        {
            try
            {
                var request = new
                {
                    OrderId = orderId,
                    CancelReason = cancelReason,
                    CancelledBy = cancelledBy
                };

                var response = await _httpClient.PostAsJsonAsync($"/api/orders/{orderId}/cancel", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                return false;
            }
        }

        public async Task<string?> GetOrderPaymentStatusAsync(Guid orderId)
        {
            try
            {
                var order = await GetOrderByIdAsync(orderId);
                return order?.PaymentStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for order {OrderId}", orderId);
                return null;
            }
        }
    }

    // DTOs for Order Service
    public class CreateMultiOrderRequest
    {
        public Guid AccountId { get; set; }
        public Guid? LivestreamId { get; set; }
        public Guid? CreatedFromCommentId { get; set; }
        public string PaymentMethod { get; set; } = "COD";
        public string? AddressId { get; set; }
        public List<CreateOrderByShopDto> OrdersByShop { get; set; } = new();
    }

    public class CreateOrderByShopDto
    {
        public Guid ShopId { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
        public decimal ShippingFee { get; set; }
        public Guid ShippingProviderId { get; set; }
        public string? CustomerNotes { get; set; }
        public string? VoucherCode { get; set; }
        public DateTime? ExpectedDeliveryDay { get; set; }
    }

    public class CreateOrderItemDto
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateMultiOrderResult
    {
        public bool Success { get; set; }
        public Guid? OrderId { get; set; }
        public string? OrderCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class OrderDetailDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public decimal FinalAmount { get; set; }
        public Guid AccountId { get; set; }
        public Guid ShopId { get; set; }
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public decimal FinalAmount { get; set; }
        public Guid AccountId { get; set; }
        public Guid ShopId { get; set; }
    }
}