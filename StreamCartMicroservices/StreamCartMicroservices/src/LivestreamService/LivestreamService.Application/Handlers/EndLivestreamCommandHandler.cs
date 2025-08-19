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

        public EndLivestreamCommandHandler(
            ILivestreamRepository livestreamRepository,
            ILogger<EndLivestreamCommandHandler> logger)
        {
            _livestreamRepository = livestreamRepository;
            _logger = logger;
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

            // End the livestream
            livestream.End(request.SellerId.ToString());
            await _livestreamRepository.ReplaceAsync(livestream.Id.ToString(), livestream);

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