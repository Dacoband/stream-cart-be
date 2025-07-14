using Livestreamservice.Application.Queries;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers
{
    public class GetShopLivestreamsQueryHandler : IRequestHandler<GetShopLivestreamsQuery, List<LivestreamDTO>>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly IAccountServiceClient _accountServiceClient;

        public GetShopLivestreamsQueryHandler(
            ILivestreamRepository livestreamRepository,
            IShopServiceClient shopServiceClient,
            IAccountServiceClient accountServiceClient)
        {
            _livestreamRepository = livestreamRepository;
            _shopServiceClient = shopServiceClient;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<List<LivestreamDTO>> Handle(GetShopLivestreamsQuery request, CancellationToken cancellationToken)
        {
            var shopLivestreams = await _livestreamRepository.GetLivestreamsByShopIdAsync(request.ShopId);
            var result = new List<LivestreamDTO>();

            foreach (var livestream in shopLivestreams)
            {
                var shop = await _shopServiceClient.GetShopByIdAsync(livestream.ShopId);
                var seller = await _accountServiceClient.GetSellerByIdAsync(livestream.SellerId);

                result.Add(new LivestreamDTO
                {
                    Id = livestream.Id,
                    Title = livestream.Title,
                    Description = livestream.Description,
                    SellerId = livestream.SellerId,
                    SellerName = seller?.Fullname ?? seller?.Username ?? "Unknown Seller",
                    ShopId = livestream.ShopId,
                    ShopName = shop?.ShopName ?? "Unknown Shop",
                    ScheduledStartTime = livestream.ScheduledStartTime,
                    ActualStartTime = livestream.ActualStartTime,
                    ActualEndTime = livestream.ActualEndTime,
                    Status = livestream.Status,
                    ThumbnailUrl = livestream.ThumbnailUrl,
                    IsPromoted = livestream.IsPromoted,
                    Tags = livestream.Tags
                });
            }

            return result;
        }
    }
}