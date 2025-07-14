using Livestreamservice.Application.DTOs;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
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
        }

        public async Task<AccountDTO> GetAccountByIdAsync(Guid accountId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/accounts/{accountId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<AccountDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return apiResponse?.Data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Account service to get account with ID {AccountId}", accountId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting account with ID {AccountId}", accountId);
                throw;
            }
        }

        public async Task<SellerDTO> GetSellerByIdAsync(Guid sellerId)
        {
            try
            {
                var serviceUrl = _configuration["ServiceUrls:AccountService"];
                var response = await _httpClient.GetAsync($"{serviceUrl}/api/accounts/{sellerId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get seller with ID {SellerId}. Status code: {StatusCode}",
                        sellerId, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<AccountDTO>>(content, _jsonOptions);

                if (apiResponse?.Data == null)
                    return null;

                // Convert account data to seller data
                return new SellerDTO
                {
                    Id = apiResponse.Data.Id,
                    Username = apiResponse.Data.Username,
                    Fullname = apiResponse.Data.Fullname,
                    AvatarUrl = apiResponse.Data.AvatarUrl,
                    ShopId = apiResponse.Data.Id, // This may need adjustment
                    //CompleteRate = 0 // This may need to be fetched separately
                };
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

        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }
    }
}