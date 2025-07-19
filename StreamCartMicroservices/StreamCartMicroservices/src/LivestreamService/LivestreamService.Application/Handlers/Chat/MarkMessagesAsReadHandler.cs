using LivestreamService.Application.Commands.Chat;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class MarkMessagesAsReadHandler : IRequestHandler<MarkMessagesAsReadCommand, bool>
    {
        private readonly IChatMessageRepository _messageRepository;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IChatNotificationService _notificationService; 
        private readonly ILogger<MarkMessagesAsReadHandler> _logger;

        public MarkMessagesAsReadHandler(
            IChatMessageRepository messageRepository,
            IChatRoomRepository chatRoomRepository,
            IChatNotificationService chatHubContext,
            ILogger<MarkMessagesAsReadHandler> logger)
        {
            _messageRepository = messageRepository;
            _chatRoomRepository = chatRoomRepository;
            _notificationService = chatHubContext;
            _logger = logger;
        }

        public async Task<bool> Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify access to chat room
                var chatRoom = await _chatRoomRepository.GetByIdAsync(request.ChatRoomId.ToString());
                if (chatRoom == null)
                {
                    return false;
                }

                await _messageRepository.MarkMessagesAsReadAsync(request.ChatRoomId, request.UserId);

                // Notify other participants that messages have been read
                await _notificationService.NotifyMessagesReadAsync(request.ChatRoomId, request.UserId, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                return false;
            }
        }
    }
}