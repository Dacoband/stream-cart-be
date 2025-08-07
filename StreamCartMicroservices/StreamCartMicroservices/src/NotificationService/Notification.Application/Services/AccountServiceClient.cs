using Microsoft.Extensions.Logging;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Services
{
    public class AccountServiceClient : IAccountServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountServiceClient> _logger;
        public AccountServiceClient(HttpClient httpClient, ILogger<AccountServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<ShopAccountDto?>> GetAccountByShopIdAsync(Guid shopId)
        {
            try
            {
                _logger.LogInformation("Fetching account for shop ID: {ShopId}", shopId);

                var response = await _httpClient.GetAsync($"https://brightpa.me/api/accounts/by-shop/{shopId}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ShopAccountDto>>>();

                if (result?.Success == true && result.Data.Count() > 0)
                {
                    return result.Data;
                }

                _logger.LogWarning("No account found for shop ID: {ShopId}", shopId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account by shop ID: {ShopId}", shopId);
                return null;
            }
        }
        public async Task<AccountDto> GetAccountByIdAsync(Guid accountId)
        {
            try
            {
                _logger.LogInformation("Getting account details for ID: {AccountId}", accountId);

                var response = await _httpClient.GetAsync($"/api/accounts/{accountId}");
                response.EnsureSuccessStatusCode();

                var account = await response.Content.ReadFromJsonAsync<AccountDto>();
                return account ?? new AccountDto { Id = accountId };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account details for ID: {AccountId}", accountId);
                throw;
            }
        }
    }
}
