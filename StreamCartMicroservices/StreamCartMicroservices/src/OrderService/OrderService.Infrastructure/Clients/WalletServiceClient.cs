using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.WalletDTOs;
using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Clients
{
    public class WalletServiceClient : IWalletServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WalletServiceClient> _logger;

        public WalletServiceClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<WalletServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Cấu hình base URL từ configuration
            var walletServiceUrl = configuration["ServiceUrls:ShopService"];
            if (!string.IsNullOrEmpty(walletServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(walletServiceUrl);
            }
        }

        public async Task ProcessShopPaymentAsync(ShopPaymentRequest paymentRequest)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"https://brightpa.me/api/wallet/shop-payment", paymentRequest);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Thanh toán cho shop thành công: Đơn hàng {OrderId}, Shop {ShopId}, Số tiền {Amount}",
                    paymentRequest.OrderId, paymentRequest.ShopId, paymentRequest.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thanh toán cho shop: Đơn hàng {OrderId}, Shop {ShopId}",
                    paymentRequest.OrderId, paymentRequest.ShopId);
                throw;
            }
        }
    }
}
