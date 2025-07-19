using LivestreamService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Services
{
    public class AccountServiceClient : IAccountServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountServiceClient> _logger;
        private readonly string _baseUrl;

        public AccountServiceClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AccountServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["ServiceUrls:AccountService"] ?? "http://account-service";
        }

        public async Task<AccountDTO> GetAccountByIdAsync(Guid accountId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/accounts/{accountId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<AccountDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Data ?? new AccountDTO();
                }

                _logger.LogWarning("Failed to get account {AccountId}: {StatusCode}", accountId, response.StatusCode);
                return new AccountDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account {AccountId}", accountId);
                return new AccountDTO();
            }
        }

        public async Task<SellerDTO?> GetSellerByIdAsync(Guid sellerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/accounts/seller/{sellerId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<SellerDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seller {SellerId}", sellerId);
                return null;
            }
        }

        public async Task<string> GetEmailByAccountIdAsync(Guid accountId)
        {
            try
            {
                var account = await GetAccountByIdAsync(accountId);
                return account?.Email ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email for account {AccountId}", accountId);
                return string.Empty;
            }
        }

        public async Task<bool> DoesUserExistAsync(Guid userId)
        {
            try
            {
                var account = await GetAccountByIdAsync(userId);
                return account != null && account.Id != Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} exists", userId);
                return false;
            }
        }

        public async Task<List<AccountDTO>> GetAccountByShopIdAsync(Guid shopId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/accounts/shop/{shopId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<AccountDTO>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Data ?? new List<AccountDTO>();
                }

                return new List<AccountDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounts for shop {ShopId}", shopId);
                return new List<AccountDTO>();
            }
        }
    }
}