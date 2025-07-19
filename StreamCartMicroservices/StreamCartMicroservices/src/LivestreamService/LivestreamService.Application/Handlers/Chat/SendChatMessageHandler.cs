using LivestreamService.Application.Commands.Chat;
using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class SendChatMessageHandler : IRequestHandler<SendChatMessageCommand, ChatMessageDTO>
    {
        private readonly IChatMessageRepository _messageRepository;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IChatNotificationService _notificationService; // ✅ Thay đổi này
        private readonly ILogger<SendChatMessageHandler> _logger;

        public SendChatMessageHandler(
            IChatMessageRepository messageRepository,
            IChatRoomRepository chatRoomRepository,
            IAccountServiceClient accountServiceClient,
            IChatNotificationService notificationService, // ✅ Thay đổi này
            ILogger<SendChatMessageHandler> logger)
        {
            _messageRepository = messageRepository;
            _chatRoomRepository = chatRoomRepository;
            _accountServiceClient = accountServiceClient;
            _notificationService = notificationService; // ✅ Thay đổi này
            _logger = logger;
        }

        public async Task<ChatMessageDTO> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify chat room exists and sender has access
                var chatRoom = await _chatRoomRepository.GetByIdAsync(request.ChatRoomId.ToString());
                if (chatRoom == null)
                {
                    throw new KeyNotFoundException("Chat room not found");
                }

                if (chatRoom.UserId != request.SenderId && chatRoom.ShopId != request.SenderId)
                {
                    // Check if sender is shop member
                    var sender = await _accountServiceClient.GetAccountByIdAsync(request.SenderId);
                    if (sender?.ShopId != chatRoom.ShopId)
                    {
                        throw new UnauthorizedAccessException("Access denied");
                    }
                }

                // Create message
                var message = new ChatMessage(
                    request.ChatRoomId,
                    request.SenderId,
                    request.Content,
                    request.MessageType,
                    request.AttachmentUrl,
                    request.SenderId.ToString()
                );

                await _messageRepository.InsertAsync(message);

                // Update chat room last message time
                chatRoom.UpdateLastMessageTime(message.SentAt, request.SenderId.ToString());
                await _chatRoomRepository.ReplaceAsync(chatRoom.Id.ToString(), chatRoom);

                // Get sender info
                var senderInfo = await _accountServiceClient.GetAccountByIdAsync(request.SenderId);

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
                    SenderName = senderInfo?.Fullname ?? senderInfo?.Username,
                    SenderAvatarUrl = senderInfo?.AvatarUrl
                };

                // ✅ Notify chat room participants using interface
                await _notificationService.NotifyChatRoomMessageAsync(request.ChatRoomId, messageDto, cancellationToken);

                return messageDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chat message");
                throw;
            }
        }
    }
}