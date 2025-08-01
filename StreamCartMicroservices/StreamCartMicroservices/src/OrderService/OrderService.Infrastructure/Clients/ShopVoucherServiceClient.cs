﻿using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Clients
{
    public class ShopVoucherServiceClient : IShopVoucherClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ShopVoucherServiceClient> _logger;
        public ShopVoucherServiceClient(HttpClient client, ILogger<ShopVoucherServiceClient> logger)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                  (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("application/json"));
            _logger = logger;
        }

        public async Task<VoucherApplicationDto> ApplyVoucherAsync(string code, Guid orderId, decimal orderAmount, string accessToken)
        {
            try
            {
                var url = $"https://brightpa.me/api/vouchers/{code}/apply";

                var requestBody = new
                {
                    OrderId = orderId,
                    OrderAmount = orderAmount
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Voucher apply failed: {StatusCode}", response.StatusCode);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<VoucherApplicationDto>>(responseContent, options);

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when applying voucher");
                return null;
            }
        }

        public async Task<VoucherValidationDto> ValidateVoucherAsync(string code, decimal orderAmount, Guid? shopId = null)
        {
            try
            {
                var baseUrl = "https://brightpa.me"; 
                var url = $"{baseUrl}/api/vouchers/{code}/validate?orderAmount={orderAmount}";

                if (shopId.HasValue)
                    url += $"&shopId={shopId}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Voucher validate failed: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<VoucherValidationDto>>(content, options);

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when validating voucher");
                return null;
            }
        }
    }
}
