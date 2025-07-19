using LivestreamService.Application.Commands.Chat;
using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using LivestreamService.Domain.Enums; // ✅ Add this using directive
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class SendLivestreamMessageHandler : IRequestHandler<SendLivestreamMessageCommand, LivestreamChatDTO>
    {
        private readonly ILivestreamChatRepository _chatRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IChatNotificationService _notificationService; // ✅ Sử dụng interface
        private readonly ILogger<SendLivestreamMessageHandler> _logger;

        public SendLivestreamMessageHandler(
            ILivestreamChatRepository chatRepository,
            IAccountServiceClient accountServiceClient,
            IChatNotificationService notificationService, // ✅ Inject interface
            ILogger<SendLivestreamMessageHandler> logger)
        {
            _chatRepository = chatRepository;
            _accountServiceClient = accountServiceClient;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<LivestreamChatDTO> Handle(SendLivestreamMessageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get sender information
                var sender = await _accountServiceClient.GetAccountByIdAsync(request.SenderId);
                if (sender == null)
                {
                    throw new ArgumentException("Sender not found");
                }

                // Determine sender type
                string senderType = sender.ShopId.HasValue && sender.ShopId != Guid.Empty ? "Shop" : "Viewer";

                // Create chat message
                var chatMessage = new LivestreamChat(
                    request.LivestreamId,
                    request.SenderId,
                    sender.Fullname ?? sender.Username ?? "Unknown",
                    senderType,
                    request.Message,
                    request.MessageType.ToString(), // ✅ Convert enum to string for domain entity
                    request.ReplyToMessageId, // ✅ Already Guid? type, matches domain entity
                    request.SenderId.ToString()
                );

                // Save to database
                await _chatRepository.InsertAsync(chatMessage);

                // Convert to DTO
                var chatDto = new LivestreamChatDTO
                {
                    Id = chatMessage.Id,
                    LivestreamId = chatMessage.LivestreamId,
                    SenderId = chatMessage.SenderId,
                    SenderName = chatMessage.SenderName,
                    SenderType = chatMessage.SenderType,
                    Message = chatMessage.Message,
                    MessageType = request.MessageType, // ✅ Use original enum value from command
                    ReplyToMessageId = chatMessage.ReplyToMessageId?.ToString(), // ✅ Convert Guid? to string?
                    IsModerated = chatMessage.IsModerated,
                    SentAt = chatMessage.SentAt,
                    CreatedAt = chatMessage.CreatedAt,
                    SenderAvatarUrl = sender.AvatarUrl
                };

                // ✅ Notify all clients in the livestream using interface
                await _notificationService.NotifyLivestreamMessageAsync(request.LivestreamId, chatDto, cancellationToken);

                return chatDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending livestream message");
                throw;
            }
        }
    }
}