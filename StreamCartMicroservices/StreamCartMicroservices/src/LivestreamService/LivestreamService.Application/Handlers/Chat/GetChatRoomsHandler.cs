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
    public class GetChatRoomsHandler : IRequestHandler<GetChatRoomsQuery, PagedResult<ChatRoomDTO>>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<GetChatRoomsHandler> _logger;

        public GetChatRoomsHandler(
            IChatRoomRepository chatRoomRepository,
            IChatMessageRepository chatMessageRepository,
            IAccountServiceClient accountServiceClient,
            IShopServiceClient shopServiceClient,
            ILogger<GetChatRoomsHandler> logger)
        {
            _chatRoomRepository = chatRoomRepository;
            _chatMessageRepository = chatMessageRepository;
            _accountServiceClient = accountServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
        }

        public async Task<PagedResult<ChatRoomDTO>> Handle(GetChatRoomsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var pagedRooms = await _chatRoomRepository.GetUserChatRoomsAsync(
                    request.UserId,
                    request.PageNumber,
                    request.PageSize,
                    request.IsActive);

                var roomDtos = new List<ChatRoomDTO>();
                foreach (var room in pagedRooms.Items)
                {
                    var user = await _accountServiceClient.GetAccountByIdAsync(room.UserId);
                    var shop = await _shopServiceClient.GetShopByIdAsync(room.ShopId);
                    var lastMessage = await _chatMessageRepository.GetLastMessageAsync(room.Id);
                    var unreadCount = await _chatMessageRepository.GetUnreadCountAsync(room.Id, request.UserId);

                    roomDtos.Add(new ChatRoomDTO
                    {
                        Id = room.Id,
                        UserId = room.UserId,
                        ShopId = room.ShopId,
                        StartedAt = room.StartedAt,
                        LastMessageAt = room.LastMessageAt,
                        RelatedOrderId = room.RelatedOrderId,
                        IsActive = room.IsActive,
                        UserName = user?.Fullname ?? user?.Username,
                        UserAvatarUrl = user?.AvatarUrl,
                        ShopName = shop?.ShopName,
                        ShopLogoUrl = shop?.LogoURL,
                        LastMessage = lastMessage != null ? new ChatMessageDTO
                        {
                            Id = lastMessage.Id,
                            Content = lastMessage.Content,
                            SentAt = lastMessage.SentAt,
                            IsRead = lastMessage.IsRead,
                            MessageType = lastMessage.MessageType
                        } : null,
                        UnreadCount = unreadCount
                    });
                }

                return new PagedResult<ChatRoomDTO>
                {
                    Items = roomDtos,
                    CurrentPage = pagedRooms.CurrentPage,
                    PageSize = pagedRooms.PageSize,
                    TotalCount = pagedRooms.TotalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat rooms");
                throw;
            }
        }
    }
}