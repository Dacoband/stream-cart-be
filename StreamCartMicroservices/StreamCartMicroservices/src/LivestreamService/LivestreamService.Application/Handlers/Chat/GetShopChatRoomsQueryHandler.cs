using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries.Chat;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Domain.Bases;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class GetShopChatRoomsQueryHandler : IRequestHandler<GetShopChatRoomsQuery, PagedResult<ChatRoomDTO>>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly ILogger<GetShopChatRoomsQueryHandler> _logger;

        public GetShopChatRoomsQueryHandler(
            IChatRoomRepository chatRoomRepository,
            ILogger<GetShopChatRoomsQueryHandler> logger)
        {
            _chatRoomRepository = chatRoomRepository;
            _logger = logger;
        }

        public async Task<PagedResult<ChatRoomDTO>> Handle(GetShopChatRoomsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting chat rooms for shop {ShopId}", request.ShopId);

                var result = await _chatRoomRepository.GetShopChatRoomsAsync(
                    request.ShopId,
                    request.PageNumber,
                    request.PageSize,
                    request.IsActive);

                // Manual mapping thay vì sử dụng AutoMapper
                var chatRoomDTOs = result.Items.Select(chatRoom => new ChatRoomDTO
                {
                    Id = chatRoom.Id,
                    UserId = chatRoom.UserId,
                    ShopId = chatRoom.ShopId,
                    StartedAt = chatRoom.StartedAt,
                    LastMessageAt = chatRoom.LastMessageAt,
                    RelatedOrderId = chatRoom.RelatedOrderId,
                    IsActive = chatRoom.IsActive,
                    UserName = chatRoom.UserName,
                    ShopName = chatRoom.ShopName,
                    LiveKitRoomName = chatRoom.LiveKitRoomName,
                    IsLiveKitActive = false, // Sẽ được cập nhật từ controller
                    UnreadCount = 0, // Có thể tính toán sau nếu cần
                    LastMessage = null, // Có thể lấy từ repository khác nếu cần
                    UserAvatarUrl = null, // Có thể lấy từ Account Service nếu cần
                    ShopLogoUrl = null // Có thể lấy từ Shop Service nếu cần
                }).ToList();

                return new PagedResult<ChatRoomDTO>(
                    chatRoomDTOs,
                    result.TotalCount,
                    result.CurrentPage,
                    result.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat rooms for shop {ShopId}", request.ShopId);
                throw;
            }
        }
    }
}