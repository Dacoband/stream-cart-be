using Livestreamservice.Application.Commands;
using LivestreamService.Application.Commands;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers
{
    public class EndLivestreamCommandHandler : IRequestHandler<EndLivestreamCommand, LivestreamDTO>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILogger<EndLivestreamCommandHandler> _logger;
        private readonly ILivestreamMembershipService _membershipService;
        private readonly IShopServiceClient _shopServiceClient;


        public EndLivestreamCommandHandler(
            ILivestreamRepository livestreamRepository,
            ILogger<EndLivestreamCommandHandler> logger,
            ILivestreamMembershipService membershipService,
            IShopServiceClient shopServiceClient)
        {
            _livestreamRepository = livestreamRepository;
            _logger = logger;
            _membershipService = membershipService;
            _shopServiceClient = shopServiceClient;
        }

        public async Task<LivestreamDTO> Handle(EndLivestreamCommand request, CancellationToken cancellationToken)
        {
            var livestream = await _livestreamRepository.GetByIdAsync(request.Id.ToString());

            if (livestream == null)
            {
                throw new KeyNotFoundException($"Livestream with ID {request.Id} not found");
            }

            if (livestream.LivestreamHostId != request.SellerId)
            {
                throw new UnauthorizedAccessException("Only the livestream creator can end it");
            }
            // Calculate used minutes
            int usedMinutes = 0;
            if (livestream.ActualStartTime.HasValue)
            {
                var livestreamDuration = DateTime.UtcNow - livestream.ActualStartTime.Value;
                usedMinutes = Math.Max(0, (int)livestreamDuration.TotalMinutes);
            }
            // End the livestream
            livestream.End(request.SellerId.ToString());
            await _livestreamRepository.ReplaceAsync(livestream.Id.ToString(), livestream);

            if (usedMinutes > 0)
            {
                var deductionSuccess = await _membershipService.DeductLivestreamTimeAsync(livestream.ShopId, usedMinutes);

                if (deductionSuccess)
                {
                    _logger.LogInformation("Ended livestream {LivestreamId} and deducted {UsedMinutes} minutes from shop {ShopId}",
                        livestream.Id, usedMinutes, livestream.ShopId);
                }
                else
                {
                    _logger.LogWarning("Ended livestream {LivestreamId} but failed to deduct {UsedMinutes} minutes from shop {ShopId}",
                        livestream.Id, usedMinutes, livestream.ShopId);
                }
            }
            _logger.LogInformation("Livestream {LivestreamId} ended by seller {SellerId}", livestream.Id, request.SellerId);

            return new LivestreamDTO
            {
                Id = livestream.Id,
                Title = livestream.Title,
                Description = livestream.Description,
                SellerId = livestream.SellerId,
                ShopId = livestream.ShopId,
                ScheduledStartTime = livestream.ScheduledStartTime,
                ActualStartTime = livestream.ActualStartTime,
                ActualEndTime = livestream.ActualEndTime,
                Status = livestream.Status,
                ThumbnailUrl = livestream.ThumbnailUrl
            };
        }
    }
}