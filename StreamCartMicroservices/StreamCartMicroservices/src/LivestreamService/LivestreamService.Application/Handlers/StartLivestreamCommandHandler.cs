using Livestreamservice.Application.Commands;
using LivestreamService.Application.Commands;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Services.User;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers
{
    public class StartLivestreamCommandHandler : IRequestHandler<StartLivestreamCommand, LivestreamDTO>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILivekitService _livekitService;
        private readonly ILogger<StartLivestreamCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        public StartLivestreamCommandHandler(
            ILivestreamRepository livestreamRepository,
            ILivekitService livekitService,
            ILogger<StartLivestreamCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _livestreamRepository = livestreamRepository;
            _livekitService = livekitService;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<LivestreamDTO> Handle(StartLivestreamCommand request, CancellationToken cancellationToken)
        {
            var livestream = await _livestreamRepository.GetByIdAsync(request.Id.ToString());

            if (livestream == null)
            {
                throw new KeyNotFoundException($"Livestream with ID {request.Id} not found");
            }

            if (livestream.LivestreamHostId != request.SellerId)
            {
                throw new UnauthorizedAccessException("Only the livestream creator can start it");
            }
             var requestingUserId = _currentUserService.GetUserId();
            // Start the livestream
            livestream.Start(request.SellerId.ToString(),requestingUserId);
            await _livestreamRepository.ReplaceAsync(livestream.Id.ToString(), livestream);

            // Generate token for the seller
            string token = await _livekitService.GenerateJoinTokenAsync(
                livestream.LivekitRoomId,
                request.SellerId.ToString(),
                true // Can publish
            );

            _logger.LogInformation("Livestream {LivestreamId} started by seller {SellerId}", livestream.Id, request.SellerId);

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
                StreamKey = livestream.StreamKey,
                LivekitRoomId = livestream.LivekitRoomId,
                JoinToken = token,
                ThumbnailUrl = livestream.ThumbnailUrl
            };
        }
    }
}