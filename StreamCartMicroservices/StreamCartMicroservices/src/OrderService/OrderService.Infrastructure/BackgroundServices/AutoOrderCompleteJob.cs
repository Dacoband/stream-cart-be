using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.WalletDTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.BackgroundServices
{
    public class AutoOrderCompleteJob : IJob
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IWalletServiceClient _walletServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<AutoOrderCompleteJob> _logger;
        private readonly string _systemUserId;

        public AutoOrderCompleteJob(
            IOrderRepository orderRepository,
            IWalletServiceClient walletServiceClient,
            IShopServiceClient shopServiceClient,
            ILogger<AutoOrderCompleteJob> logger,
            IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _walletServiceClient = walletServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
            _systemUserId = configuration["SystemAccounts:SystemUserId"] ?? "system";
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                // Lấy danh sách đơn hàng đã giao nhưng chưa được xác nhận trong 3 ngày
                var thresholdDate = DateTime.UtcNow.AddDays(-3);
                var pendingOrders = await _orderRepository.GetShippedOrdersBeforeDateAsync(thresholdDate);

                _logger.LogInformation("Đã tìm thấy {Count} đơn hàng cần tự động hoàn thành", pendingOrders.Count());

                foreach (var order in pendingOrders)
                {
                    try
                    {
                        // Cập nhật trạng thái đơn hàng
                        order.UpdateStatus(OrderStatus.Delivered, _systemUserId);
                        await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                        // Xử lý thanh toán cho shop
                        await ProcessPaymentToShopAsync(order);

                        _logger.LogInformation("Đã tự động hoàn thành đơn hàng {OrderId}", order.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi tự động hoàn thành đơn hàng {OrderId}", order.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện công việc tự động hoàn thành đơn hàng");
            }
        }
        private async Task ProcessPaymentToShopAsync(Orders order)
        {
            // Tính toán số tiền thanh toán cho shop (trừ 10% phí)
            decimal totalAmount = order.TotalPrice;
            decimal fee = totalAmount * 0.1m; // 10% phí
            decimal amountToShop = totalAmount - fee;

            // Gửi yêu cầu thanh toán đến WalletService
            var paymentRequest = new ShopPaymentRequest
            {
                OrderId = order.Id,
                ShopId = order.ShopId,
                Amount = amountToShop,
                Fee = fee,
                TransactionType = "OrderCompleteAuto",
                TransactionReference = order.Id.ToString(),
                Description = $"Tự động thanh toán đơn hàng #{order.OrderCode} sau 3 ngày"
            };

            await _walletServiceClient.ProcessShopPaymentAsync(paymentRequest);

            // Cập nhật tỷ lệ hoàn thành đơn hàng của shop
            await _shopServiceClient.UpdateShopCompletionRateAsync(
                order.ShopId,
                0.5m, // Tăng 0.5% cho mỗi đơn hàng hoàn thành
                Guid.Parse(_systemUserId)
            );
        }
    }
}
