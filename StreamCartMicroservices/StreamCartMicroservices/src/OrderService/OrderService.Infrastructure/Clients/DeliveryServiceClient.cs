using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Delivery;
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
    public class DeliveryServiceClient : IDeliveryClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DeliveryServiceClient> _logger;

        public DeliveryServiceClient(HttpClient httpClient, ILogger<DeliveryServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> CancelDeliveryOrderAsync(string deliveryCode)
        {
            try
            {
                var url = "https://brightpa.me/api/delivery/cancel";

                var content = new StringContent(JsonSerializer.Serialize(deliveryCode), Encoding.UTF8, "application/json");


                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Cancel delivery failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new ApiResponse<bool> { Success = false, Message = "Hủy đơn giao hàng thất bại" };
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<ApiResponse<bool>>(responseContent, options);

                return result ?? new ApiResponse<bool> { Success = false, Message = "Phản hồi không hợp lệ từ dịch vụ giao hàng" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when cancelling delivery");
                return new ApiResponse<bool> { Success = false, Message = "Lỗi khi hủy đơn giao hàng" };
            }
        }

        public async Task<ApiResponse<CreateOrderResult>> CreateGhnOrderAsync(UserCreateOrderRequest request)
        {
            try
            {
                var url = "https://brightpa.me/api/deliveries/create-ghn-order";

                // ✅ Log request payload để debug
                _logger.LogInformation("Creating GHN order with request: {@Request}", request);

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                _logger.LogDebug("GHN Request JSON: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("GHN Response: Status={StatusCode}, Content={Content}",
                    response.StatusCode, responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("GHN order creation failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new ApiResponse<CreateOrderResult> { Success = false, Message = "Tạo đơn hàng GHN thất bại" };
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<CreateOrderResult>>(responseContent, options);

                return apiResponse ?? new ApiResponse<CreateOrderResult> { Success = false, Message = "Phản hồi không hợp lệ từ GHN" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while calling create-ghn-order");
                return new ApiResponse<CreateOrderResult> { Success = false, Message = "Lỗi khi gọi GHN API" };
            }
        }
    }
 }
