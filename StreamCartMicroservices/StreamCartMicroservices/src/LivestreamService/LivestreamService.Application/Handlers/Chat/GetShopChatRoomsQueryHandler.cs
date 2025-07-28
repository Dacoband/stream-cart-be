using AutoMapper;
using LivestreamService.Application.DTOs.Chat;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries.Chat;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Chat
{
    public class GetShopChatRoomsQueryHandler : IRequestHandler<GetShopChatRoomsQuery, PagedResult<ChatRoomDTO>>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetShopChatRoomsQueryHandler> _logger;

        public GetShopChatRoomsQueryHandler(
            IChatRoomRepository chatRoomRepository,
            IMapper mapper,
            ILogger<GetShopChatRoomsQueryHandler> logger)
        {
            _chatRoomRepository = chatRoomRepository;
            _mapper = mapper;
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

                var chatRoomDTOs = _mapper.Map<IEnumerable<ChatRoomDTO>>(result.Items);

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