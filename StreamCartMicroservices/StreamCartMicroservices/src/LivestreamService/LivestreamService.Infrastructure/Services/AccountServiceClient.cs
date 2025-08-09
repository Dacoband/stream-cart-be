using LivestreamService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Services
{
    public class AccountServiceClient : IAccountServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountServiceClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IConfiguration _configuration;

        public AccountServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<AccountServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var accountServiceUrl = _configuration["ServiceUrls:AccountService"];
            if (!string.IsNullOrEmpty(accountServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(accountServiceUrl);
                _logger.LogInformation("AccountServiceClient configured with base URL: {BaseUrl}", accountServiceUrl);
            }
            else
            {
                _logger.LogWarning("ServiceUrls:AccountService is not configured. HTTP requests may fail.");
            }
        }

        public async Task<AccountDTO> GetAccountByIdAsync(Guid accountId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/accounts/{accountId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<AccountDTO>>(content, _jsonOptions);

                return apiResponse?.Data ?? new AccountDTO();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Account service to get account with ID {AccountId}", accountId);
                return new AccountDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting account with ID {AccountId}", accountId);
                return new AccountDTO(); 
            }
        }

        public async Task<SellerDTO?> GetSellerByIdAsync(Guid sellerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/accounts/{sellerId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<SellerDTO>>(content, _jsonOptions);

                return apiResponse?.Data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Account service to get seller with ID {SellerId}", sellerId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting seller with ID {SellerId}", sellerId);
                throw;
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
                var response = await _httpClient.GetAsync($"/api/accounts/shop/{shopId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<AccountDTO>>>(content, _jsonOptions);

                return apiResponse?.Data ?? new List<AccountDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Account service to get accounts for shop {ShopId}", shopId);
                return new List<AccountDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting accounts for shop {ShopId}", shopId);
                throw;
            }
        }

        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }
    }
}