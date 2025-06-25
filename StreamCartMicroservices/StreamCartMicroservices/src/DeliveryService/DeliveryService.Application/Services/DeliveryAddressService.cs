using DeliveryService.Application.DTOs.AddressDTOs;
using DeliveryService.Application.DTOs.BaseDTOs;
using DeliveryService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeliveryService.Application.Services
{
    public class DeliveryAddressService : IDeliveryAddressInterface
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly GHNSettings _ghnSettings;

        public DeliveryAddressService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IOptions<GHNSettings> ghnOptions)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _ghnSettings = ghnOptions.Value;
        }

        private HttpRequestMessage CreateRequest(string url)
        {
            var token = _ghnSettings.Token;
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Token", token);
            return request;
        }
        public async Task<List<GHNDistrictDTO>> GetDistrictsAsync(int provinceId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(CreateRequest($"https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/district?province_id={provinceId}"));
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GHNResponseDTO<List<GHNDistrictDTO>>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ); return result?.Data ?? new();
        }

        public async Task<List<GHNProvinceDTO>> GetProvincesAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(CreateRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/province"));
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GHNResponseDTO<List<GHNProvinceDTO>>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            return result?.Data ?? new();
        }

        public async Task<List<GHNWardDTO>> GetWardsAsync(int districtId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(CreateRequest($"https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/ward?district_id={districtId}"));
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GHNResponseDTO<List<GHNWardDTO>>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            return result?.Data ?? new();
        }

        public Task<int?> FindProvinceIdByNameAsync(string provinceName)
        {
            throw new NotImplementedException();
        }

        public Task<int?> FindDistrictIdByNameAsync(string districtName, int provinceId)
        {
            throw new NotImplementedException();
        }

        public Task<string?> FindWardCodeByNameAsync(string wardName, int districtId)
        {
            throw new NotImplementedException();
        }
    }
}
