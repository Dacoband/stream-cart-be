using DeliveryService.Application.DTOs.AccountDTO;
using DeliveryService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeliveryService.Application.Services
{
    public class AddressClientService : IAddressClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AddressClientService> _logger;
        public AddressClientService(HttpClient client, ILogger<AddressClientService> logger)
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
        public async Task<AddressDto?> GetShopAddress(string shopId)
        {
            try
            {
                var url = $"https://brightpa.me/api/addresses/shops/{shopId}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Raw JSON response: {content}"); // Log raw JSON

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var addressResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<AddressDto>>>(content, options);
                var addressList = addressResponse?.Data;
                var address = addressList?.Where(x => x.IsDefaultShipping == true).FirstOrDefault();
                if (address == null)
                {
                    _logger.LogError("Deserialization returned null");
                    return null;
                }

                
                return address;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
