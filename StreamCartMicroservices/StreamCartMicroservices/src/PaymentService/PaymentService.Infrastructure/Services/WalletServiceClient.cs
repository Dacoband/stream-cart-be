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
        private readonly JsonSerializerOptions _jsonOptions;

        public WalletServiceClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<WalletServiceClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _walletServiceBaseUrl = _configuration["Services:WalletService:BaseUrl"]
                ?? "https://brightpa.me";

            // ✅ Khởi tạo _jsonOptions
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        public async Task<bool> CreateWalletTransactionAsync(CreateWalletTransactionRequest request)
        {
            try
            {
                var requestBody = new
                {
                    type = request.Type,
                    amount = request.Amount,
                    shopId = request.ShopId, 
                    description = request.Description,
                    status = request.Status,
                    transactionId = request.TransactionId,
                    createdBy = request.CreatedBy
                };

                var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_walletServiceBaseUrl}/api/shop-wallet",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Tạo wallet transaction thành công cho shop {ShopId}", request.ShopId);
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
                // ✅ Sử dụng _walletServiceBaseUrl để có URL đầy đủ
                var response = await _httpClient.GetAsync($"{_walletServiceBaseUrl}/api/shop-wallet/{transactionId}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                // ✅ Sử dụng _jsonOptions đã được khởi tạo
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
                var requestBody = new
                {
                    Status = status,
                    PaymentTransactionId = paymentTransactionId,
                    ModifiedBy = modifiedBy ?? "System"
                };

                // ✅ Sử dụng _jsonOptions đã được khởi tạo
                var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
                // ✅ Fix StringContent constructor
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // ✅ Sử dụng _walletServiceBaseUrl để có URL đầy đủ
                var response = await _httpClient.PatchAsync($"{_walletServiceBaseUrl}/api/shop-wallet/{transactionId}", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating wallet transaction status {TransactionId}", transactionId);
                return false;
            }
        }
    }
}