using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries.Chat;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class GetUnreadMessagesCountQueryHandler : IRequestHandler<GetUnreadMessagesCountQuery, Dictionary<Guid, int>>
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly ILogger<GetUnreadMessagesCountQueryHandler> _logger;

        public GetUnreadMessagesCountQueryHandler(
            IChatMessageRepository chatMessageRepository,
            IChatRoomRepository chatRoomRepository,
            ILogger<GetUnreadMessagesCountQueryHandler> logger)
        {
            _chatMessageRepository = chatMessageRepository;
            _chatRoomRepository = chatRoomRepository;
            _logger = logger;
        }

        public async Task<Dictionary<Guid, int>> Handle(GetUnreadMessagesCountQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = new Dictionary<Guid, int>();

                // Lấy tất cả chat rooms của user
                var userChatRooms = await _chatRoomRepository.GetChatRoomsByUserIdAsync(request.UserId);

                // Đếm số lượng tin nhắn chưa đọc trong mỗi chat room
                foreach (var chatRoom in userChatRooms)
                {
                    var unreadCount = await _chatMessageRepository.GetUnreadCountAsync(chatRoom.Id, request.UserId);
                    if (unreadCount > 0)
                    {
                        result[chatRoom.Id] = unreadCount;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread message count for user {UserId}", request.UserId);
                throw;
            }
        }
    }
}