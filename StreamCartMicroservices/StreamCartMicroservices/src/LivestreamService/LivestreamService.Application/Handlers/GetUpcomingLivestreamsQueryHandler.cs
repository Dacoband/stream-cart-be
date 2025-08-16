using Livestreamservice.Application.Queries;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers
{
    public class GetUpcomingLivestreamsQueryHandler : IRequestHandler<GetUpcomingLivestreamsQuery, List<LivestreamDTO>>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly IAccountServiceClient _accountServiceClient;

        public GetUpcomingLivestreamsQueryHandler(
            ILivestreamRepository livestreamRepository,
            IShopServiceClient shopServiceClient,
            IAccountServiceClient accountServiceClient)
        {
            _livestreamRepository = livestreamRepository;
            _shopServiceClient = shopServiceClient;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<List<LivestreamDTO>> Handle(GetUpcomingLivestreamsQuery request, CancellationToken cancellationToken)
        {
            var upcomingLivestreams = await _livestreamRepository.GetUpcomingLivestreamsAsync();

            if (request.IncludePromotedOnly)
            {
                upcomingLivestreams = upcomingLivestreams.Where(l => l.IsPromoted).ToList();
            }

            var result = new List<LivestreamDTO>();

            foreach (var livestream in upcomingLivestreams)
            {
                var shop = await _shopServiceClient.GetShopByIdAsync(livestream.ShopId);
                var seller = await _accountServiceClient.GetSellerByIdAsync(livestream.SellerId);
                var livestreamHost = await _accountServiceClient.GetAccountByIdAsync(livestream.LivestreamHostId);

                result.Add(new LivestreamDTO
                {
                    Id = livestream.Id,
                    Title = livestream.Title,
                    Description = livestream.Description,
                    SellerId = livestream.SellerId,
                    SellerName = seller?.Fullname ?? seller?.Username ?? "Unknown Seller",
                    ShopId = livestream.ShopId,
                    LivestreamHostId = livestream.LivestreamHostId,
                    ShopName = shop?.ShopName ?? "Unknown Shop",
                    LivestreamHostName = livestreamHost?.Fullname ?? livestreamHost?.Username,
                    ScheduledStartTime = livestream.ScheduledStartTime,
                    ActualStartTime = livestream.ActualStartTime,
                    ActualEndTime = livestream.ActualEndTime,
                    Status = livestream.Status,
                    ThumbnailUrl = livestream.ThumbnailUrl,
                    MaxViewer = livestream.MaxViewer,
                    ApprovalStatusContent = livestream.ApprovalStatusContent,
                    ApprovedByUserId = livestream.ApprovedByUserId,
                        
                    ApprovalDateContent = livestream.ApprovalDateContent,
                    StreamKey = livestream.StreamKey,
                    PlaybackUrl = livestream.PlaybackUrl,
                    LivekitRoomId = livestream.LivekitRoomId,
                    JoinToken = livestream.JoinToken,
                    IsPromoted = livestream.IsPromoted,
                    Tags = livestream.Tags
                });
            }

            return result;
        }
    }
}
