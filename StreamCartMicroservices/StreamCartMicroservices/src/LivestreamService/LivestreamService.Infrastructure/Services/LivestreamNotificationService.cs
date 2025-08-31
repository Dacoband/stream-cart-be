using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Enums;
using LivestreamService.Infrastructure.Interfaces;
using LivestreamService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Services
{
    public class LivestreamNotificationService : ILivestreamNotificationService
    {
        private readonly ISignalRChatService _signalRChatService; 
        private readonly ILogger<LivestreamNotificationService> _logger;

        public LivestreamNotificationService(
            ISignalRChatService signalRChatService,
            ILogger<LivestreamNotificationService> logger)
        {
            _signalRChatService = signalRChatService;
            _logger = logger;
        }

        public async Task SendLivestreamTimeWarningAsync(Guid livestreamId, Guid sellerId, int remainingMinutes)
        {
            try
            {
                var warningMessage = new LivestreamChatDTO
                {
                    Id = Guid.NewGuid(),
                    LivestreamId = livestreamId,
                    SenderId = Guid.Empty, // System message
                    SenderName = "🤖 Hệ thống StreamCart",
                    SenderType = "System",
                    Message = $"⚠️ CẢNH BÁO: Livestream sẽ kết thúc trong {remainingMinutes} phút do hết thời gian gói thành viên. Vui lòng chuẩn bị kết thúc livestream hoặc gia hạn gói thành viên!",
                    MessageType = MessageType.System,
                    IsModerated = false,
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _signalRChatService.NotifyNewLivestreamMessageAsync(sellerId, warningMessage);

                await _signalRChatService.SendMessageToLivestreamAsync(
                    livestreamId,
                    Guid.Empty,
                    "🤖 Hệ thống",
                    $"⚠️ Livestream sẽ kết thúc trong {remainingMinutes} phút");


                _logger.LogInformation("Sent livestream time warning to seller {SellerId} for livestream {LivestreamId}. Remaining: {RemainingMinutes} minutes",
                    sellerId, livestreamId, remainingMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending livestream time warning for livestream {LivestreamId}", livestreamId);
            }
        }

        public async Task SendLivestreamTimeExpiredAsync(Guid livestreamId, Guid sellerId)
        {
            try
            {
                var expiredMessage = new LivestreamChatDTO
                {
                    Id = Guid.NewGuid(),
                    LivestreamId = livestreamId,
                    SenderId = Guid.Empty, // System message
                    SenderName = "🤖 Hệ thống StreamCart",
                    SenderType = "System",
                    Message = "⛔ THÔNG BÁO: Livestream đã hết thời gian theo gói thành viên và sẽ được tự động kết thúc ngay bây giờ. Cảm ơn bạn đã sử dụng StreamCart!",
                    MessageType = MessageType.System,
                    IsModerated = false,
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // ✅ FIX: Gửi trực tiếp qua SignalRChatService
                await _signalRChatService.NotifyNewLivestreamMessageAsync(sellerId, expiredMessage);

                // ✅ BONUS: Broadcast cho tất cả viewers
                await _signalRChatService.SendMessageToLivestreamAsync(
                    livestreamId,
                    Guid.Empty,
                    "🤖 Hệ thống",
                    "⛔ Livestream đã kết thúc do hết thời gian gói thành viên. Cảm ơn mọi người đã theo dõi!");
                _logger.LogInformation("Sent livestream time expired notification to seller {SellerId} for livestream {LivestreamId}",
                    sellerId, livestreamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending livestream time expired notification for livestream {LivestreamId}", livestreamId);
            }
        }
    }
}