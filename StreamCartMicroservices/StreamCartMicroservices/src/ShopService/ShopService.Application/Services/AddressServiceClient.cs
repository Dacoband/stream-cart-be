using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Common.Models;
using ShopService.Application.DTOs.Address;
using ShopService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Services
{
    public class AddressServiceClient : IAddressServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AddressServiceClient> _logger;
        public AddressServiceClient(HttpClient httpClient, ILogger<AddressServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<ApiResponse<AddressDto>> CreateAddressAsync(CreateAddressDto dto, string token)
        {
            try
            {
                // Gắn token (nếu API yêu cầu xác thực)
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("https://brightpa.me/api/addresses", dto);

                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ApiResponse<AddressDto>>(content);
                    return result!;
                }
                else
                {
                    _logger.LogError("Error creating address: {Content}", content);
                    return ApiResponse<AddressDto>.ErrorResult("Failed to create address");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while calling CreateAddress API");
                return ApiResponse<AddressDto>.ErrorResult("Exception: " + ex.Message);
            }
        
    }
    }
}
