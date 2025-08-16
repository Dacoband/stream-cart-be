using Livestreamservice.Application.Queries;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers
{
    public class GetLivestreamByIdQueryHandler : IRequestHandler<GetLivestreamByIdQuery, LivestreamDTO>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly IAccountServiceClient _accountServiceClient;

        public GetLivestreamByIdQueryHandler(
            ILivestreamRepository livestreamRepository,
            IShopServiceClient shopServiceClient,
            IAccountServiceClient accountServiceClient)
        {
            _livestreamRepository = livestreamRepository;
            _shopServiceClient = shopServiceClient;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<LivestreamDTO> Handle(GetLivestreamByIdQuery request, CancellationToken cancellationToken)
        {
            var livestream = await _livestreamRepository.GetByIdAsync(request.Id.ToString());
            if (livestream == null)
            {
                return null;
            }

            var shop = await _shopServiceClient.GetShopByIdAsync(livestream.ShopId);
            var seller = await _accountServiceClient.GetSellerByIdAsync(livestream.SellerId);
            var livestreamHost = await _accountServiceClient.GetAccountByIdAsync(livestream.LivestreamHostId);

            return new LivestreamDTO
            {
                Id = livestream.Id,
                Title = livestream.Title,
                Description = livestream.Description,
                SellerId = livestream.SellerId,
                SellerName = seller?.Fullname ?? seller?.Username ?? "Unknown Seller",
                ShopId = livestream.ShopId,
                ShopName = shop?.ShopName ?? "Unknown Shop",
                LivestreamHostId = livestream.LivestreamHostId,
                LivestreamHostName = livestreamHost?.Fullname ?? livestreamHost?.Username,
                ScheduledStartTime = livestream.ScheduledStartTime,
                ActualStartTime = livestream.ActualStartTime,
                ActualEndTime = livestream.ActualEndTime,
                Status = livestream.Status,
                StreamKey = livestream.StreamKey,
                PlaybackUrl = livestream.PlaybackUrl,
                ThumbnailUrl = livestream.ThumbnailUrl,
                MaxViewer = livestream.MaxViewer,
                ApprovalStatusContent = livestream.ApprovalStatusContent,
                ApprovedByUserId = livestream.ApprovedByUserId,
                ApprovalDateContent = livestream.ApprovalDateContent,
                IsPromoted = livestream.IsPromoted,
                Tags = livestream.Tags,
                LivekitRoomId = livestream.LivekitRoomId,
                JoinToken = livestream.JoinToken,
            };
        }
    }
}