using Microsoft.Extensions.Logging;
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

        // ✅ FIX: Update method signature to match interface
        public async Task<VoucherApplicationDto> ApplyVoucherAsync(string code, Guid orderId, decimal orderAmount, Guid shopId, string accessToken)
        {
            try
            {
                var url = $"https://brightpa.me/api/vouchers/{code}/apply";

                var requestBody = new
                {
                    Code = code,
                    OrderId = orderId,
                    OrderAmount = orderAmount,
                    ShopId = shopId
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // ✅ ADD: Enhanced logging
                _logger.LogInformation("🎫 Sending voucher apply request - URL: {Url}, Body: {Body}", url, json);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.PostAsync(url, content);

                // ✅ FIX: Always read response content first, then check success
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("📨 Voucher apply response - Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("❌ Voucher apply failed with {StatusCode}: {Content}",
                        response.StatusCode, responseContent);
                    return new VoucherApplicationDto
                    {
                        IsApplied = false,
                        Message = $"Lỗi áp dụng voucher: {response.StatusCode} - {responseContent}",
                        DiscountAmount = 0,
                        FinalAmount = orderAmount
                    };
                }

                // ✅ Parse the successful response
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<VoucherApplicationDto>>(responseContent, options);

                    if (apiResponse?.Success == true && apiResponse.Data != null)
                    {
                        _logger.LogInformation("✅ Voucher {Code} applied successfully for shop {ShopId}: discount {Discount}đ",
                            code, shopId, apiResponse.Data.DiscountAmount);
                        return apiResponse.Data;
                    }

                    // Handle case where success=false in response
                    _logger.LogWarning("⚠️ API returned success=false: {Message}", apiResponse?.Message);
                    return new VoucherApplicationDto
                    {
                        IsApplied = false,
                        Message = apiResponse?.Message ?? "Voucher application failed",
                        DiscountAmount = 0,
                        FinalAmount = orderAmount
                    };
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "❌ Failed to deserialize voucher response: {Content}", responseContent);
                    return new VoucherApplicationDto
                    {
                        IsApplied = false,
                        Message = "Lỗi xử lý response từ server",
                        DiscountAmount = 0,
                        FinalAmount = orderAmount
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception when applying voucher {Code} for shop {ShopId}", code, shopId);
                return new VoucherApplicationDto
                {
                    IsApplied = false,
                    Message = $"Lỗi hệ thống: {ex.Message}",
                    DiscountAmount = 0,
                    FinalAmount = orderAmount
                };
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
                    _logger.LogError("Voucher validate failed: {StatusCode} for code {Code}, shop {ShopId}",
                        response.StatusCode, code, shopId);
                    return new VoucherValidationDto
                    {
                        IsValid = false,
                        Message = $"Lỗi kiểm tra voucher: {response.StatusCode}",
                        DiscountAmount = 0,
                        FinalAmount = orderAmount
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<VoucherValidationDto>>(content, options);

                if (apiResponse?.Data != null)
                {
                    _logger.LogInformation("✅ Voucher {Code} validation for shop {ShopId}: valid={Valid}, discount={Discount}đ",
                        code, shopId, apiResponse.Data.IsValid, apiResponse.Data.DiscountAmount);
                    return apiResponse.Data;
                }

                return new VoucherValidationDto
                {
                    IsValid = false,
                    Message = "Không thể kiểm tra voucher",
                    DiscountAmount = 0,
                    FinalAmount = orderAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when validating voucher {Code} for shop {ShopId}", code, shopId);
                return new VoucherValidationDto
                {
                    IsValid = false,
                    Message = $"Lỗi hệ thống: {ex.Message}",
                    DiscountAmount = 0,
                    FinalAmount = orderAmount
                };
            }
        }
    }
}