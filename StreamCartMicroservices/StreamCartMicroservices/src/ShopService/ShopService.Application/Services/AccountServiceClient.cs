﻿using Microsoft.Extensions.Configuration;
using Shared.Common.Models;
using ShopService.Application.DTOs;
using ShopService.Application.DTOs.Account;
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
       public async Task<AccountDetailDTO> GetAccountByAccountIdAsync(Guid accountId)
{
            var response = await _httpClient.GetFromJsonAsync<Shared.Common.Models.ApiResponse<AccountDetailDTO>>($"api/accounts/{accountId}");

            if (response == null || !response.Success || response.Data == null)
    {
        throw new Exception($"Không thể lấy thông tin tài khoản {accountId}");
    }

    return response.Data;
    }
        public async Task<List<ShopAccountDTO>> GetAccountsByShopIdAsync(Guid shopId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Shared.Common.Models.ApiResponse<List<ShopAccountDTO>>>($"api/accounts/by-shop/{shopId}");

                if (response == null || !response.Success || response.Data == null)
                {
                    return new List<ShopAccountDTO>();
                }

                return response.Data;
            }
            catch (Exception)
            {
                // Trả về danh sách rỗng nếu có lỗi
                return new List<ShopAccountDTO>();
            }
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