using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class AccountClientService : IAccountCLientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountClientService> _logger;

       
        public AccountClientService(HttpClient httpClient, ILogger<AccountClientService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<AccountDto?> GetAccountById(string accountId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://brightpa.me/api/accounts/{accountId}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<AccountDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse?.Data;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
