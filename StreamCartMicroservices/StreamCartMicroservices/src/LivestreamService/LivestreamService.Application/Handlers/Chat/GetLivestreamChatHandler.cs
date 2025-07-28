using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries.Chat;
using LivestreamService.Domain.Enums; // ✅ Add this using directive
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class GetLivestreamChatHandler : IRequestHandler<GetLivestreamChatQuery, PagedResult<LivestreamChatDTO>>
    {
        private readonly ILivestreamChatRepository _chatRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly ILogger<GetLivestreamChatHandler> _logger;

        public GetLivestreamChatHandler(
            ILivestreamChatRepository chatRepository,
            IAccountServiceClient accountServiceClient,
            ILogger<GetLivestreamChatHandler> logger)
        {
            _chatRepository = chatRepository;
            _accountServiceClient = accountServiceClient;
            _logger = logger;
        }

        public async Task<PagedResult<LivestreamChatDTO>> Handle(GetLivestreamChatQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var pagedMessages = await _chatRepository.GetLivestreamChatAsync(
                    request.LivestreamId,
                    request.PageNumber,
                    request.PageSize,
                    request.IncludeModerated);

                var messageDtos = new List<LivestreamChatDTO>();
                foreach (var message in pagedMessages.Items)
                {
                    var sender = await _accountServiceClient.GetAccountByIdAsync(message.SenderId);

                    // ✅ Convert string MessageType to enum
                    MessageType messageTypeEnum = MessageType.Text; // Default value
                    if (Enum.TryParse<MessageType>(message.MessageType, true, out var parsedType))
                    {
                        messageTypeEnum = parsedType;
                    }

                    messageDtos.Add(new LivestreamChatDTO
                    {
                        Id = message.Id,
                        LivestreamId = message.LivestreamId,
                        SenderId = message.SenderId,
                        SenderName = message.SenderName,
                        SenderType = message.SenderType,
                        Message = message.Message,
                        MessageType = messageTypeEnum, // ✅ Now properly converted from string to enum
                        ReplyToMessageId = message.ReplyToMessageId?.ToString(), // ✅ Convert Guid? to string?
                        IsModerated = message.IsModerated,
                        SentAt = message.SentAt,
                        CreatedAt = message.CreatedAt,
                        SenderAvatarUrl = sender?.AvatarUrl
                    });
                }

                return new PagedResult<LivestreamChatDTO>
                {
                    Items = messageDtos,
                    CurrentPage = pagedMessages.CurrentPage,
                    PageSize = pagedMessages.PageSize,
                    TotalCount = pagedMessages.TotalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream chat");
                throw;
            }
        }
    }
}