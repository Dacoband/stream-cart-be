using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries.Chat;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class GetChatRoomHandler : IRequestHandler<GetChatRoomQuery, ChatRoomDTO?>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<GetChatRoomHandler> _logger;

        public GetChatRoomHandler(
            IChatRoomRepository chatRoomRepository,
            IChatMessageRepository chatMessageRepository,
            IAccountServiceClient accountServiceClient,
            IShopServiceClient shopServiceClient,
            ILogger<GetChatRoomHandler> logger)
        {
            _chatRoomRepository = chatRoomRepository;
            _chatMessageRepository = chatMessageRepository;
            _accountServiceClient = accountServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
        }

        public async Task<ChatRoomDTO?> Handle(GetChatRoomQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var chatRoom = request.ChatRoomId.HasValue
                    ? await _chatRoomRepository.GetByIdAsync(request.ChatRoomId.Value.ToString())
                    : await _chatRoomRepository.GetByUserAndShopAsync(request.UserId, request.ShopId);

                if (chatRoom == null)
                {
                    return null;
                }
                if (request.RequesterId.HasValue)
                {
                    // Check if requester is either the customer or belongs to the shop
                    var hasAccess = chatRoom.UserId == request.RequesterId.Value;

                    if (!hasAccess)
                    {
                        // Check if requester is from the shop (you may need to implement shop membership check)
                        // For now, we'll allow access if ShopId matches (this should be enhanced with proper shop membership validation)
                        var requesterAccount = await _accountServiceClient.GetAccountByIdAsync(request.RequesterId.Value);
                        if (requesterAccount?.ShopId != chatRoom.ShopId)
                        {
                            _logger.LogWarning("Access denied. Requester {RequesterId} does not have access to chat room {ChatRoomId}",
                                request.RequesterId.Value, chatRoom.Id);
                            return null;
                        }
                    }
                }
                var user = await _accountServiceClient.GetAccountByIdAsync(chatRoom.UserId);
                var shop = await _shopServiceClient.GetShopByIdAsync(chatRoom.ShopId);
                var lastMessage = await _chatMessageRepository.GetLastMessageAsync(chatRoom.Id);
                var unreadCount = await _chatMessageRepository.GetUnreadCountAsync(chatRoom.Id, request.UserId);

                return new ChatRoomDTO
                {
                    Id = chatRoom.Id,
                    UserId = chatRoom.UserId,
                    ShopId = chatRoom.ShopId,
                    StartedAt = chatRoom.StartedAt,
                    LastMessageAt = chatRoom.LastMessageAt,
                    RelatedOrderId = chatRoom.RelatedOrderId,
                    IsActive = chatRoom.IsActive,
                    UserName = user?.Fullname ?? user?.Username,
                    UserAvatarUrl = user?.AvatarUrl,
                    ShopName = shop?.ShopName,
                    ShopLogoUrl = shop?.LogoURL,
                    LastMessage = lastMessage != null ? new ChatMessageDTO
                    {
                        Id = lastMessage.Id,
                        ChatRoomId = lastMessage.ChatRoomId,
                        SenderUserId = lastMessage.SenderUserId,
                        Content = lastMessage.Content,
                        SentAt = lastMessage.SentAt,
                        IsRead = lastMessage.IsRead,
                        MessageType = lastMessage.MessageType
                    } : null,
                    UnreadCount = unreadCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat room");
                return null;
            }
        }
    }
}