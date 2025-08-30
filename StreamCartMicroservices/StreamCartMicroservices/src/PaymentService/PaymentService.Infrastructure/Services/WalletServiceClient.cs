using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using Shared.Common.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PaymentService.Infrastructure.Services
{
    /// <summary>
    /// Client để gọi WalletService
    /// </summary>
    public class WalletServiceClient : IWalletServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WalletServiceClient> _logger;
        private readonly string _walletServiceBaseUrl;
        private readonly IHttpContextAccessor _httpContextAccessor; 
        private readonly JsonSerializerOptions _jsonOptions;

        public WalletServiceClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<WalletServiceClient> logger,IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _walletServiceBaseUrl = _configuration["Services:WalletService:BaseUrl"]
                ?? "https://brightpa.me";
            _httpContextAccessor = httpContextAccessor;

            // ✅ Khởi tạo _jsonOptions
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
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

                    _logger.LogInformation("Set authorization header for wallet service call");
                }
                else
                {
                    _logger.LogWarning("No authorization header found in current request");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set authorization header");
            }
        }
        public async Task<bool> CreateWalletTransactionAsync(CreateWalletTransactionRequest request)
        {
            try
            {
                SetAuthorizationHeader();
                var requestBody = new
                {
                    type = request.Type,                    
                    amount = request.Amount,
                    description = request.Description ?? $"Nạp tiền vào ví shop {request.ShopId}",
                    status = request.Status,                
                    transactionId = request.TransactionId,
                    shopMembershipId = (Guid?)null,
                    orderId = (Guid?)null,
                    refundId = (Guid?)null
                };

                var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request_message = new HttpRequestMessage(HttpMethod.Post, $"{_walletServiceBaseUrl}/api/shop-wallet")
                {
                    Content = content
                };

                request_message.Headers.Add("X-Shop-Id", request.ShopId.ToString());
                request_message.Headers.Add("X-User-Id", request.CreatedBy);

                var response = await _httpClient.SendAsync(request_message);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Tạo wallet transaction thành công cho shop {ShopId}. Response: {Response}",
                        request.ShopId, responseContent);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Tạo wallet transaction thất bại cho shop {ShopId}. Status: {StatusCode}, Error: {Error}",
                    request.ShopId, response.StatusCode, errorContent);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo wallet transaction cho shop {ShopId}", request.ShopId);
                return false;
            }
        }

        public async Task<bool> DoesShopExistAsync(Guid shopId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_walletServiceBaseUrl}/api/wallets/shop/{shopId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra shop {ShopId}", shopId);
                return false;
            }
        }

        public async Task<WalletTransactionDto?> GetWalletTransactionByIdAsync(Guid transactionId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_walletServiceBaseUrl}/api/shop-wallet/{transactionId}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<WalletTransactionDto>>(json, _jsonOptions);

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet transaction {TransactionId}", transactionId);
                return null;
            }
        }

        public async Task<bool> UpdateWalletTransactionStatusAsync(Guid transactionId, int status, string? paymentTransactionId = null, string? modifiedBy = null)
        {
            try
            {
                SetAuthorizationHeader();
                var requestBody = new
                {
                    status = status 
                };

                var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"{_walletServiceBaseUrl}/api/shop-wallet/{transactionId}")
                {
                    Content = content
                };
                if (!string.IsNullOrEmpty(paymentTransactionId))
                {
                    requestMessage.Headers.Add("X-Payment-Transaction-Id", paymentTransactionId);
                }

                if (!string.IsNullOrEmpty(modifiedBy))
                {
                    requestMessage.Headers.Add("X-Modified-By", modifiedBy);
                }

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Cập nhật trạng thái wallet transaction thành công: {TransactionId} -> {Status}",
                        transactionId, status);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Cập nhật trạng thái wallet transaction thất bại: {TransactionId}. Status: {StatusCode}, Error: {Error}",
                    transactionId, response.StatusCode, errorContent);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái wallet transaction {TransactionId}", transactionId);
                return false;
            }
        }
        public async Task<bool> UpdateWalletBalanceAsync(Guid shopId, decimal amount, string modifiedBy = "System")
        {
            try
            {
                SetAuthorizationHeader();

                var requestBody = new
                {
                    amount = amount,
                    modifiedBy = modifiedBy
                };

                var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync($"{_walletServiceBaseUrl}/api/shop-wallet/shop/{shopId}/balance", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Cập nhật balance wallet thành công cho shop {ShopId}, amount {Amount}", shopId, amount);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Cập nhật balance wallet thất bại cho shop {ShopId}. Status: {StatusCode}, Error: {Error}",
                    shopId, response.StatusCode, errorContent);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật balance wallet cho shop {ShopId}", shopId);
                return false;
            }
        }
        private static string MapTypeToEnum(int type)
        {
            return type switch
            {
                0 => "Withdraw",    // WalletTransactionType.Withdraw
                1 => "Deposit",     // WalletTransactionType.Deposit  
                2 => "Commission",  // WalletTransactionType.Commission
                3 => "System",      // WalletTransactionType.System
                _ => "Deposit"      // Default to Deposit
            };
        }
        private static string MapStatusToEnum(int status)
        {
            return status switch
            {
                0 => "Success",     // WalletTransactionStatus.Success
                1 => "Failed",      // WalletTransactionStatus.Failed
                2 => "Pending",     // WalletTransactionStatus.Pending
                3 => "Canceled",    // WalletTransactionStatus.Canceled
                _ => "Pending"      // Default to Pending
            };
        }
    }
}