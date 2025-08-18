using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Shared.Common.Models;

namespace OrderService.Infrastructure.Clients
{
    /// <summary>
    /// Implementation of the IAccountServiceClient interface
    /// </summary>
    public class AccountServiceClient : IAccountServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountServiceClient> _logger;

        /// <summary>
        /// Creates a new instance of AccountServiceClient
        /// </summary>
        /// <param name="httpClient">HTTP client</param>
        /// <param name="logger">Logger</param>
        public AccountServiceClient(HttpClient httpClient, ILogger<AccountServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets account details by ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Account details</returns>
        public async Task<AccountDto> GetAccountByIdAsync(Guid accountId)
        {
            try
            {
                _logger.LogInformation("Getting account details for ID: {AccountId}", accountId);

                var response = await _httpClient.GetAsync($"/api/accounts/{accountId}");
                response.EnsureSuccessStatusCode();

                // ✅ FIX: Đọc qua ApiResponse wrapper
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>();

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    _logger.LogInformation("✅ Successfully retrieved account {AccountId}: {FullName}, Avatar: {Avatar}",
                        accountId, apiResponse.Data.FullName, apiResponse.Data.AvatarURL);
                    return apiResponse.Data;
                }

                _logger.LogWarning("⚠️ Account {AccountId} not found or API returned unsuccessful response", accountId);
                return new AccountDto { Id = accountId };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting account details for ID: {AccountId}", accountId);

                // ✅ Return fallback instead of throwing
                return new AccountDto
                {
                    Id = accountId,
                    FullName = "Unknown User",
                    AvatarURL = null
                };
            }
        }

        /// <summary>
        /// Gets email address by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Email address</returns>
        public async Task<string> GetEmailByAccountIdAsync(Guid accountId)
        {
            try
            {
                var account = await GetAccountByIdAsync(accountId);
                return account?.Email ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email for account ID: {AccountId}", accountId);
                return string.Empty;
            }
        }

        public async Task<IEnumerable<ShopAccountDto?>> GetAccountByShopIdAsync(Guid shopId)
        {
            try
            {
                _logger.LogInformation("Fetching account for shop ID: {ShopId}", shopId);

                var response = await _httpClient.GetAsync($"/api/accounts/by-shop/{shopId}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ShopAccountDto>>>();

                if (result?.Success == true && result.Data?.Count() > 0)
                {
                    return result.Data;
                }

                _logger.LogWarning("No account found for shop ID: {ShopId}", shopId);
                return Enumerable.Empty<ShopAccountDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account by shop ID: {ShopId}", shopId);
                return Enumerable.Empty<ShopAccountDto>();
            }
        }
    }
}