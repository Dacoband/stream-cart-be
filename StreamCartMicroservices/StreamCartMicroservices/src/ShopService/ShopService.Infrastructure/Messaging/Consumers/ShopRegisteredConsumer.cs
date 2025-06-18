using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Common.Services.Email;
using Shared.Messaging.Consumers;
using ShopService.Application.DTOs;
using ShopService.Application.Events;
using ShopService.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace ShopService.Infrastructure.Messaging.Consumers
{
    public class ShopRegisteredConsumer : IConsumer<ShopRegistered>, IBaseConsumer
    {
        private readonly ILogger<ShopRegisteredConsumer> _logger;
        private readonly IEmailService _emailService;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IAdminNotificationService _notificationService;

        public ShopRegisteredConsumer(
            ILogger<ShopRegisteredConsumer> logger,
            IEmailService emailService,
            IAccountServiceClient accountServiceClient,
            IAdminNotificationService notificationService)
        {
            _logger = logger;
            _emailService = emailService;
            _accountServiceClient = accountServiceClient;
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<ShopRegistered> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Shop registered: {ShopId}, Name: {ShopName}, Account: {AccountId}, Date: {RegistrationDate}",
                message.ShopId,
                message.ShopName,
                message.AccountId,
                message.RegistrationDate);

            try
            {
                // 1. Cập nhật thông tin ShopId trong Account
                await _accountServiceClient.UpdateAccountShopInfoAsync(
                    message.AccountId, message.ShopId);

                // 2. Gửi email chào mừng
                var ownerEmail = await _accountServiceClient.GetEmailByAccountIdAsync(message.AccountId);
                if (!string.IsNullOrEmpty(ownerEmail))
                {
                    await _emailService.SendEmailAsync(
                        to: ownerEmail,
                        subject: $"Chào mừng cửa hàng {message.ShopName} tham gia nền tảng!",
                        htmlBody: GenerateWelcomeEmailTemplate(message),
                        toName: message.ShopName
                    );
                }

                // 3. Thông báo cho admin xét duyệt
                await _notificationService.SendApprovalRequestAsync(
                    new ApprovalRequestDto
                    {
                        EntityId = message.ShopId,
                        EntityType = "Shop",
                        EntityName = message.ShopName,
                        RequestDate = DateTime.UtcNow
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shop registration event for ShopId: {ShopId}",
                    message.ShopId);
            }
        }

        private string GenerateWelcomeEmailTemplate(ShopRegistered shop)
        {
            return $@"
            <h2>Chào mừng {shop.ShopName}!</h2>
            <p>Cảm ơn bạn đã đăng ký cửa hàng với Stream Cart.</p>
            <p>Cửa hàng của bạn hiện đang chờ phê duyệt. Chúng tôi sẽ thông báo cho bạn ngay khi quá trình xét duyệt hoàn tất.</p>
            <p>Trân trọng,<br/>Đội ngũ Stream Cart</p>
        ";
        }
    }
}