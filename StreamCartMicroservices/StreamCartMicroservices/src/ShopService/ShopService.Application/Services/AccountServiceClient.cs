using Microsoft.Extensions.Configuration;
using ShopService.Application.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ShopService.Application.Services
{
    public class AccountServiceClient : IAccountServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AccountServiceClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            
            // Cấu hình base URL từ configuration
            var accountServiceUrl = _configuration["ServiceUrls:AccountService"];
            if (!string.IsNullOrEmpty(accountServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(accountServiceUrl);
            }
        }

        public async Task UpdateAccountShopInfoAsync(Guid accountId, Guid shopId)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/accounts/{accountId}/shop", new { 
                ShopId = shopId 
            });
            
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> GetEmailByAccountIdAsync(Guid accountId)
        {
            var account = await _httpClient.GetFromJsonAsync<AccountDto>($"api/accounts/{accountId}");
            return account?.Email ?? string.Empty;
        }
    }

    // DTO nội bộ để nhận thông tin tài khoản từ API
    internal class AccountDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}