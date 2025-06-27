using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Services
{
    public class AccountServiceClient : IAccountServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountServiceClient> _logger;

        public AccountServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<AccountServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var accountServiceUrl = configuration["ServiceUrls:AccountService"];
            if (!string.IsNullOrEmpty(accountServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(accountServiceUrl);
            }
        }

        public async Task<bool> DoesUserExistAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/accounts/{userId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra tài khoản {UserId}", userId);
                return false;
            }
        }

        //public async Task<string> GetEmailByAccountIdAsync(Guid accountId)
        //{
        //    try
        //    {
        //        var account = await _httpClient.GetFromJsonAsync<AccountDto>($"api/accounts/{accountId}");
        //        return account?.Email ?? string.Empty;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lỗi khi lấy email của tài khoản {AccountId}", accountId);
        //        return string.Empty;
        //    }
        //}
    }
}
