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
    public class CreateChatRoomHandler : IRequestHandler<CreateChatRoomCommand, ChatRoomDTO>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<CreateChatRoomHandler> _logger;

        public CreateChatRoomHandler(
            IChatRoomRepository chatRoomRepository,
            IChatMessageRepository chatMessageRepository,
            IAccountServiceClient accountServiceClient,
            IShopServiceClient shopServiceClient,
            ILogger<CreateChatRoomHandler> logger)
        {
            _chatRoomRepository = chatRoomRepository;
            _chatMessageRepository = chatMessageRepository;
            _accountServiceClient = accountServiceClient;
            _shopServiceClient = shopServiceClient;
            _logger = logger;
        }

        public async Task<ChatRoomDTO> Handle(CreateChatRoomCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if chat room already exists
                var existingRoom = await _chatRoomRepository.GetByUserAndShopAsync(request.UserId, request.ShopId);
                if (existingRoom != null)
                {
                    if (!existingRoom.IsActive)
                    {
                        existingRoom.ReactivateRoom(request.UserId.ToString());
                        await _chatRoomRepository.ReplaceAsync(existingRoom.Id.ToString(), existingRoom);
                    }
                    existingRoom.UpdateChatRoomInfo(
                       //liveKitRoomName: request.LiveKitRoomName,
                       //customerToken: request.CustomerToken,
                       userName: request.UserName,
                       shopName: request.ShopName,
                       modifiedBy: request.UserId.ToString()
                   );
                    await _chatRoomRepository.ReplaceAsync(existingRoom.Id.ToString(), existingRoom);


                    return await MapToChatRoomDTO(existingRoom, request.UserId);
                }
                string? userName = request.UserName;
                string? shopName = request.ShopName;

                if (string.IsNullOrEmpty(userName))
                {
                    var user = await _accountServiceClient.GetAccountByIdAsync(request.UserId);
                    userName = user?.Fullname ?? user?.Username;
                }

                if (string.IsNullOrEmpty(shopName))
                {
                    var shop = await _shopServiceClient.GetShopByIdAsync(request.ShopId);
                    shopName = shop?.ShopName;
                }
                // Create new chat room
                var chatRoom = new ChatRoom(
                    request.UserId,
                    request.ShopId,
                    //request.LiveKitRoomName,
                    //request.CustomerToken,
                    userName,
                    shopName,
                    request.RelatedOrderId,
                    request.UserId.ToString()
                );

                await _chatRoomRepository.InsertAsync(chatRoom);

                // Send initial message if provided
                if (!string.IsNullOrEmpty(request.InitialMessage))
                {
                    var initialMessage = new ChatMessage(
                        chatRoom.Id,
                        request.UserId,
                        request.InitialMessage,
                        "Text",
                        null,
                        request.UserId.ToString()
                    );

                    await _chatMessageRepository.InsertAsync(initialMessage);
                    chatRoom.UpdateLastMessageTime(initialMessage.SentAt, request.UserId.ToString());
                    await _chatRoomRepository.ReplaceAsync(chatRoom.Id.ToString(), chatRoom);
                }

                return await MapToChatRoomDTO(chatRoom, request.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat room");
                throw;
            }
        }

        private async Task<ChatRoomDTO> MapToChatRoomDTO(ChatRoom chatRoom, Guid requesterId)
        {
            var user = await _accountServiceClient.GetAccountByIdAsync(chatRoom.UserId);
            var shop = await _shopServiceClient.GetShopByIdAsync(chatRoom.ShopId);
            var lastMessage = await _chatMessageRepository.GetLastMessageAsync(chatRoom.Id);
            var unreadCount = await _chatMessageRepository.GetUnreadCountAsync(chatRoom.Id, requesterId);

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
    }
}