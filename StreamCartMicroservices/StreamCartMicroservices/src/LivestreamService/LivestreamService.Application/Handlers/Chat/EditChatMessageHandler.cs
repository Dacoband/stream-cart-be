using LivestreamService.Application.Commands.Chat;
using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class EditChatMessageHandler : IRequestHandler<EditChatMessageCommand, ChatMessageDTO>
    {
        private readonly IChatMessageRepository _messageRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IChatNotificationService _notificationService; 
        private readonly ILogger<EditChatMessageHandler> _logger;

        public EditChatMessageHandler(
            IChatMessageRepository messageRepository,
            IAccountServiceClient accountServiceClient,
            IChatNotificationService chatNotificationService,
            ILogger<EditChatMessageHandler> logger)
        {
            _messageRepository = messageRepository;
            _accountServiceClient = accountServiceClient;
            _notificationService = chatNotificationService;
            _logger = logger;
        }

        public async Task<ChatMessageDTO> Handle(EditChatMessageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var message = await _messageRepository.GetByIdAsync(request.MessageId.ToString());
                if (message == null)
                {
                    throw new KeyNotFoundException("Message not found");
                }

                if (message.SenderUserId != request.UserId)
                {
                    throw new UnauthorizedAccessException("Only message sender can edit");
                }

                message.EditMessage(request.Content, request.UserId.ToString());
                await _messageRepository.ReplaceAsync(message.Id.ToString(), message);

                var sender = await _accountServiceClient.GetAccountByIdAsync(message.SenderUserId);

                var messageDto = new ChatMessageDTO
                {
                    Id = message.Id,
                    ChatRoomId = message.ChatRoomId,
                    SenderUserId = message.SenderUserId,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsRead = message.IsRead,
                    IsEdited = message.IsEdited,
                    MessageType = message.MessageType,
                    AttachmentUrl = message.AttachmentUrl,
                    EditedAt = message.EditedAt,
                    SenderName = sender?.Fullname ?? sender?.Username,
                    SenderAvatarUrl = sender?.AvatarUrl,
                    IsMine = true
                };

                // Notify chat room participants
                await _notificationService.NotifyMessageEditAsync(message.ChatRoomId, messageDto, cancellationToken);

                return messageDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing chat message");
                throw;
            }
        }
    }
}