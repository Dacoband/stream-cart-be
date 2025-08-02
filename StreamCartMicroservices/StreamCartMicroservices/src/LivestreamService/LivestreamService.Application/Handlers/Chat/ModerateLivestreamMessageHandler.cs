using LivestreamService.Application.Commands.Chat;
using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class ModerateLivestreamMessageHandler : IRequestHandler<ModerateLivestreamMessageCommand, LivestreamChatDTO>
    {
        private readonly ILivestreamChatRepository _chatRepository;
        private readonly IChatNotificationService _notificationService;
        private readonly ILogger<ModerateLivestreamMessageHandler> _logger;

        public ModerateLivestreamMessageHandler(
            ILivestreamChatRepository chatRepository,
            IChatNotificationService notificationService,
            ILogger<ModerateLivestreamMessageHandler> logger)
        {
            _chatRepository = chatRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<LivestreamChatDTO> Handle(ModerateLivestreamMessageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var message = await _chatRepository.GetByIdAsync(request.MessageId.ToString());
                if (message == null)
                {
                    throw new Exception($"Không tìm thấy tin nhắn với ID: {request.MessageId}");
                }

                if (request.IsModerated)
                {
                    message.Moderate(request.ModeratorId, request.ModeratorId.ToString());
                }
                else
                {
                    message.Unmoderate(request.ModeratorId.ToString());
                }

                await _chatRepository.ReplaceAsync(message.Id.ToString(), message);

                // Notify clients about moderation using interface
                await _notificationService.NotifyMessageModerationAsync(
                    message.LivestreamId,
                    request.MessageId,
                    request.IsModerated,
                    cancellationToken);

                // Chuyển đổi và trả về LivestreamChatDTO
                return new LivestreamChatDTO
                {
                    Id = message.Id,
                    LivestreamId = message.LivestreamId,
                    SenderId = message.SenderId,
                    SenderName = message.SenderName,
                    SenderType = message.SenderType,
                    Message = message.Message,
                    MessageType = Enum.Parse<MessageType>(message.MessageType),
                    ReplyToMessageId = message.ReplyToMessageId?.ToString(),
                    IsModerated = message.IsModerated,
                    SentAt = message.SentAt,
                    CreatedAt = message.CreatedAt,
                    // Các thông tin bổ sung nếu có
                    SenderAvatarUrl = null, // Cần bổ sung thêm nếu message có thông tin này
                    ReplyToMessage = null, // Cần bổ sung thêm nếu có
                    ReplyToSenderName = null // Cần bổ sung thêm nếu có
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating livestream message");
                throw; // Chuyển từ return false sang throw để controller có thể xử lý
            }
        }
    }
}