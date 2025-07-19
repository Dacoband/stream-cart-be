using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries.Chat;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class GetChatMessagesHandler : IRequestHandler<GetChatMessagesQuery, PagedResult<ChatMessageDTO>>
    {
        private readonly IChatMessageRepository _messageRepository;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly ILogger<GetChatMessagesHandler> _logger;

        public GetChatMessagesHandler(
            IChatMessageRepository messageRepository,
            IChatRoomRepository chatRoomRepository,
            IAccountServiceClient accountServiceClient,
            ILogger<GetChatMessagesHandler> logger)
        {
            _messageRepository = messageRepository;
            _chatRoomRepository = chatRoomRepository;
            _accountServiceClient = accountServiceClient;
            _logger = logger;
        }

        public async Task<PagedResult<ChatMessageDTO>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify access to chat room
                var chatRoom = await _chatRoomRepository.GetByIdAsync(request.ChatRoomId.ToString());
                if (chatRoom == null)
                {
                    throw new KeyNotFoundException("Chat room not found");
                }

                // Check if requester has access
                var requester = await _accountServiceClient.GetAccountByIdAsync(request.RequesterId);
                if (chatRoom.UserId != request.RequesterId &&
                    requester?.ShopId != chatRoom.ShopId)
                {
                    throw new UnauthorizedAccessException("Access denied");
                }

                var pagedMessages = await _messageRepository.GetChatRoomMessagesAsync(
                    request.ChatRoomId,
                    request.PageNumber,
                    request.PageSize);

                var messageDtos = new List<ChatMessageDTO>();
                foreach (var message in pagedMessages.Items)
                {
                    var sender = await _accountServiceClient.GetAccountByIdAsync(message.SenderUserId);

                    messageDtos.Add(new ChatMessageDTO
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
                        IsMine = message.SenderUserId == request.RequesterId
                    });
                }

                return new PagedResult<ChatMessageDTO>
                {
                    Items = messageDtos,
                    CurrentPage = pagedMessages.CurrentPage,
                    PageSize = pagedMessages.PageSize,
                    TotalCount = pagedMessages.TotalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat messages");
                throw;
            }
        }
    }
}