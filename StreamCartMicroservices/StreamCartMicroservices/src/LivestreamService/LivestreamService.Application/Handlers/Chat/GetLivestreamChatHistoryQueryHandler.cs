using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries.Chat;
using LivestreamService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Domain.Bases;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class GetLivestreamChatHistoryQueryHandler : IRequestHandler<GetLivestreamChatHistoryQuery, PagedResult<LivestreamChatDTO>>
    {
        private readonly ILivestreamChatRepository _chatRepository;
        private readonly ILogger<GetLivestreamChatHistoryQueryHandler> _logger;

        public GetLivestreamChatHistoryQueryHandler(
            ILivestreamChatRepository chatRepository,
            ILogger<GetLivestreamChatHistoryQueryHandler> logger)
        {
            _chatRepository = chatRepository;
            _logger = logger;
        }

        public async Task<PagedResult<LivestreamChatDTO>> Handle(GetLivestreamChatHistoryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting livestream chat history for livestream {LivestreamId}", request.LivestreamId);

                // Use the existing method from the repository
                var messages = await _chatRepository.GetLivestreamChatAsync(
                    request.LivestreamId,
                    request.PageNumber,
                    request.PageSize,
                    request.IncludeModerated);

                // Convert domain entities to DTOs
                var chatDTOs = messages.Items.Select(message => new LivestreamChatDTO
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
                    // Additional fields for display - can be enhanced later
                    SenderAvatarUrl = null,
                    ReplyToMessage = null,
                    ReplyToSenderName = null
                }).ToList();

                var result = new PagedResult<LivestreamChatDTO>(
                    chatDTOs,
                    messages.TotalCount,
                    messages.CurrentPage,
                    messages.PageSize);

                _logger.LogInformation("Retrieved {Count} chat messages for livestream {LivestreamId}",
                    chatDTOs.Count, request.LivestreamId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream chat history for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}