using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Shared.Common.Models;
using ShopService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Clients
{
    public class AddressServiceClient : IAdressServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AddressServiceClient> _logger;
        public AddressServiceClient(HttpClient client, ILogger<AddressServiceClient> logger)
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
        public async Task<AdressDto> GetCustomerAddress(string id)
        {
            try
            {
                var url = $"https://brightpa.me/api/addresses/{id}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Raw JSON response: {content}"); // Log raw JSON

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var addressResponse = JsonSerializer.Deserialize<ApiResponse<AdressDto>>(content, options);
                var address = addressResponse?.Data;
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
