using Appwrite;
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
using System.Web;

namespace OrderService.Infrastructure.Clients
{
    public class MembershipServiceClient : IMembershipServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MembershipServiceClient> _logger;
        public MembershipServiceClient(HttpClient client, ILogger<MembershipServiceClient> logger)
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
        public async Task<DetailMembershipDTO> GetShopMembershipDTO(string shopId)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["ShopId"] = shopId;
            //query["MembershipType"] = "New";
            query["Status"] = "Ongoing";
            try
            {
                var url = $"https://brightpa.me/api/shopmembership/filter?{query}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Filter API returned status code {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Raw JSON response: {content}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ListShopMembershipDTO>>(content, options);
                return apiResponse.Data.DetailShopMembership[0];

            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
