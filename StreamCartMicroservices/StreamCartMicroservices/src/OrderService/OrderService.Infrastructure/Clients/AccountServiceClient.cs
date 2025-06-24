using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

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
                
                var account = await response.Content.ReadFromJsonAsync<AccountDto>();
                return account ?? new AccountDto { Id = accountId };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account details for ID: {AccountId}", accountId);
                throw;
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
    }
}