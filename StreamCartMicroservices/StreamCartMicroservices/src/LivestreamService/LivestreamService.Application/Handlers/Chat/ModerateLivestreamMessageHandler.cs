// Thay thế import và dependency
using LivestreamService.Application.Commands.Chat;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class ModerateLivestreamMessageHandler : IRequestHandler<ModerateLivestreamMessageCommand, bool>
    {
        private readonly ILivestreamChatRepository _chatRepository;
        private readonly IChatNotificationService _notificationService; // ✅ Thay đổi
        private readonly ILogger<ModerateLivestreamMessageHandler> _logger;

        public ModerateLivestreamMessageHandler(
            ILivestreamChatRepository chatRepository,
            IChatNotificationService notificationService, // ✅ Thay đổi
            ILogger<ModerateLivestreamMessageHandler> logger)
        {
            _chatRepository = chatRepository;
            _notificationService = notificationService; // ✅ Thay đổi
            _logger = logger;
        }

        public async Task<bool> Handle(ModerateLivestreamMessageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var message = await _chatRepository.GetByIdAsync(request.MessageId.ToString());
                if (message == null)
                {
                    return false;
                }

                if (request.IsModerated)
                {
                    // ✅ Use correct method name from LivestreamChat class
                    message.Moderate(request.ModeratorId, request.ModeratorId.ToString());
                }
                else
                {
                    // ✅ Use correct method name from LivestreamChat class
                    message.Unmoderate(request.ModeratorId.ToString());
                }

                await _chatRepository.ReplaceAsync(message.Id.ToString(), message);

                // ✅ Notify clients about moderation using interface
                await _notificationService.NotifyMessageModerationAsync(
                    message.LivestreamId,
                    request.MessageId,
                    request.IsModerated,
                    cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating livestream message");
                return false;
            }
        }
    }
}