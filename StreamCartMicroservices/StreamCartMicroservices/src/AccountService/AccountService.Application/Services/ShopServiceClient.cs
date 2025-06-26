using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AccountService.Application.Services
{
    public class ShopServiceClient : IShopServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ShopServiceClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            
            var shopServiceUrl = _configuration["ServiceUrls:ShopService"];
            if (!string.IsNullOrEmpty(shopServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(shopServiceUrl);
            }
        }

        public async Task<ShopDto> GetShopByIdAsync(Guid shopId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/shops/{shopId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ShopDto>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<ShopDto>> GetShopsByAccountIdAsync(Guid accountId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/shops/my-shops?accountId={accountId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<IEnumerable<ShopDto>>();
            }
            catch
            {
                return Array.Empty<ShopDto>();
            }
        }

        public async Task<bool> HasShopPermissionAsync(Guid accountId, Guid shopId, string role)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/shops/{shopId}/permission?accountId={accountId}&role={role}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}